using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using NeptuneEVO.SDK;
using NeptuneEVO;

namespace Golemo.Families
{
    class Commands
    {
        private static readonly nLog Log = new nLog("FamilyCommands");
        public static void InviteToFamily(Player sender, Player target)
        {
            try
            {
                if (sender.Position.DistanceTo(target.Position) > 3)
                {
                    Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок слишком далеко от Вас", 3000);
                    return;
                }
                if (Manager.isHaveFamily(target))
                {
                    Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок уже состоит в организации", 3000);
                    return;
                }
                // if (Main.Players[target].LVL < 1)
                // {
                    // Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"Необходим как минимум 1 lvl для приглашения игрока", 3000);
                    // return;
                // }
                if (Main.Players[target].Warns > 0)
                {
                    Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"Невозможно принять этого игрока", 3000);
                    return;
                }

                target.SetData("INVITEFAMILY", Main.Players[sender].FamilyCID);
                target.SetData("SENDERFAMILY", sender);
                NeptuneEVO.Trigger.PlayerEvent(target, "openDialog", "INVITEDTOFAMILY", $"{sender.Name} пригласил Вас в {Family.GetFamilyName(sender)}");

                Notify.Send(sender, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы пригласили в семью {target.Name}", 3000);
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
            }
        }

        public static void SetFamilyRank(Player sender, Player target, int newrank)
        {
            try
            {
                if (newrank <= 0)
                {
                    Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, "Нельзя установить отрицательный или нулевой ранг", 3000);
                    return;
                }
                int senderlvl = Main.Players[sender].FamilyRank;
                int playerlvl = Main.Players[target].FamilyRank;
                string senderfamily = Main.Players[sender].FamilyCID;
                if (!Manager.isHaveFamily(target, senderfamily)) return;

                if (newrank >= senderlvl)
                {
                    Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете повысить до этого ранга", 3000);
                    return;
                }
                if (playerlvl > senderlvl)
                {
                    Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете изменять ранг этого игрока", 3000);
                    return;
                };
                if (sender == target)
                {
                    Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете изменять свой ранг", 3000);
                    return;
                }
                Family family = Family.GetFamilyToCid(senderfamily);
                int memberindex = family.Players.FindIndex(x => x.Name == target.Name);

                Main.Players[target].FamilyRank = newrank;
                Member.LoadMembers(target, Main.Players[target].FamilyCID, Main.Players[target].FamilyRank);
                int uuid = Main.Players[target].UUID;
                if (Manager.AllMembers.ContainsKey(uuid))
                {
                    Manager.AllMembers[uuid].FamilyRank = newrank;
                    Manager.AllMembers[uuid].FamilyRankName = Ranks.GetFamilyRankName(senderfamily, newrank);
                }
                if (memberindex > -1)
                {
                    family.Players[memberindex].FamilyRank = newrank;
                    family.Players[memberindex].FamilyRankName = Ranks.GetFamilyRankName(senderfamily, newrank);
                }
                Notify.Send(target, NotifyType.Warning, NotifyPosition.BottomCenter, $"Теперь Вы член семьи {Manager.Members[target].FamilyRankName} ", 3000);
                Notify.Send(sender, NotifyType.Warning, NotifyPosition.BottomCenter, $"Вы изменили ранг игрока {target.Name} на {Manager.Members[target].FamilyRankName}", 3000);

                Main.Players[target].Save(target).Wait();
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
            }

        }

        internal static void DeleteFamilyMember(Player sender, Player target, string reason)
        {
            try
            {
                Family family = Family.GetFamilyToCid(Main.Players[sender].FamilyCID);
                string senderfamily = family.FamilyCID;
                int senderlvl = Main.Players[sender].FamilyRank;
                int playerlvl = Main.Players[target].FamilyRank;

                if (!Manager.isHaveFamily(target, senderfamily)) return;

                if (playerlvl >= senderlvl)
                {
                    Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете кикнуть этого игрока", 3000);
                    return;
                };
                if (sender == target)
                {
                    Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете кикнуть самого себя", 3000);
                    return;
                }
                Member.UnLoadMember(target);
                string msg = reason == null || reason == "" ? "Без причины" : $"По причине: {reason}";
                Notify.Send(target, NotifyType.Warning, NotifyPosition.BottomCenter, $"Вас выгнали из {family.Name}. {msg}", 3000);
                Notify.Send(sender, NotifyType.Warning, NotifyPosition.BottomCenter, $"Вы выгнали игрока {target.Name} из {family.Name}", 3000);
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
            }
            
        }
    }
}
