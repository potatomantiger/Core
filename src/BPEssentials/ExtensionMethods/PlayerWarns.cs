﻿using BrokeProtocol.API.Types;
using BrokeProtocol.Entities;
using BrokeProtocol.LiteDB;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace BPEssentials.ExtensionMethods.Warns
{
    public static class ExtensionPlayerWarns
    {
        public static string CustomDataKey { get; } = "bpe:warns";

        public class SerializableWarn
        {
            public string Reason { get; set; }

            public bool Expired { get; set; }

            public string IssueraccountID { get; set; }

            public DateTimeOffset Date { get; set; }

            public int Length { get; set; }

            public SerializableWarn(string issuer, string reason, DateTimeOffset dateTime, int length = -1)
            {
                IssueraccountID = issuer;
                Reason = reason;
                Date = dateTime;
                Length = length > 0 ? length : Core.Instance.Settings.Warns.DefaultWarnsExpirationInDays;
            }

            public string ToString(ShPlayer player)
            {
                var issuer = Core.Instance.SvManager.database.Users.FindById(IssueraccountID);
                return player.T("warn_toString", Reason, issuer?.ID ?? IssueraccountID, Date.ToUniversalTime().ToString(CultureInfo.InvariantCulture), Length, Expired ? player.T("warn_expired") : "");
            }
        }

        public static void AddWarn(this ShPlayer player, ShPlayer issuer, string reason, int length = -1)
        {
            player.svPlayer.CustomData.AddWarn(issuer, reason, length);
        }

        public static void AddWarn(this User user, ShPlayer issuer, string reason, int length = -1)
        {
            user.Character.CustomData.AddWarn(issuer, reason, length);
        }

        public static void AddWarn(this CustomData customData, ShPlayer issuer, string reason, int length = -1)
        {
            var warns = customData.GetWarns();
            warns.Add(new SerializableWarn(issuer.username, reason, DateTimeOffset.Now, length));
            customData.AddOrUpdate(CustomDataKey, warns);
        }

        public static void RemoveWarn(this ShPlayer player, int warnId)
        {
            player.svPlayer.CustomData.RemoveWarn(warnId);
        }

        public static void RemoveWarn(this User user, int warnId)
        {
            user.Character.CustomData.RemoveWarn(warnId);
        }

        public static void RemoveWarn(this CustomData customData, int warnId)
        {
            var warns = customData.GetWarns();
            warns.Remove(warns[warnId]);
            customData.AddOrUpdate(CustomDataKey, warns);
        }

        public static List<SerializableWarn> GetWarns(this ShPlayer player)
        {
            return player.svPlayer.CustomData.GetWarns();

        }

        public static List<SerializableWarn> GetWarns(this User user)
        {
            return user.Character.CustomData.GetWarns();
        }

        private static List<SerializableWarn> GetWarns(this CustomData customData)
        {
            if (!customData.TryFetchCustomData<List<SerializableWarn>>(CustomDataKey, out var warns))
            {
                return new List<SerializableWarn>();
            }
            // Checking for expired Warns
            for (int i = warns.Count - 1; i >= 0; i--)
            {
                if (warns[i].Expired)
                {
                    continue;
                }
                if (warns[i].Date.AddDays(warns[i].Length) > DateTimeOffset.Now)
                {
                    continue;
                }
                if (Core.Instance.Settings.Warns.DeleteExpiredWarns)
                {
                    warns.RemoveAt(i);
                    continue;
                }
                warns[i].Expired = true;
            }

            return warns;
        }
    }
}
