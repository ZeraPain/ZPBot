using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ZPBot.Annotations;
using ZPBot.Common.Resources;

namespace ZPBot.Common.Party
{
    internal class PartyManager : ThreadManager
    {
        public bool AutoAccept { get; set; }
        public bool AutoInvite { get; set; }
        public bool AcceptInviteAll { get; set; }
        public int PartyType { get; set; }
        public List<string> AcceptInviteList { get; protected set; }

        [CanBeNull]
        public Party CurrentParty { get; protected set; }

        private readonly GlobalManager _globalManager;

        public PartyManager(GlobalManager globalManager)
        {
            _globalManager = globalManager;
            AcceptInviteList = new List<string>();
        }

        public void PartyRequest(uint worldIdPlayer, byte partyType)
        {
            if (AutoAccept && (partyType%4 == PartyType))
            {
                if (AcceptInviteAll)
                {
                    _globalManager.PacketManager.AcceptPlayerRequest();
                    return;
                }

                var inviter = _globalManager.CharManager.GetCharById(worldIdPlayer);
                if (inviter == null)
                    return;

                if (AcceptInviteList.Any(player => inviter.Charname == player))
                {
                    _globalManager.PacketManager.AcceptPlayerRequest();
                    return;
                }

                _globalManager.PacketManager.DenyPlayerRequest();
            }
            else
            {
                _globalManager.PacketManager.DenyPlayerRequest();
            }
        }

        public void JoinParty(uint partyId, uint masterId, byte partyType, List<PartyMember> partyMembers)
        {
            CurrentParty = new Party(partyId, masterId, partyType, partyMembers);
            _globalManager.FMain.UpdateParty(CurrentParty.PartyMembers);
        }

        public void Dismiss()
        {
            CurrentParty = null;
            _globalManager.FMain.UpdateParty(null);
        }

        public void Join(PartyMember partyMember)
        {
            if (CurrentParty == null)
                return;

            CurrentParty.Add(partyMember);
            _globalManager.FMain.UpdateParty(CurrentParty.PartyMembers);
        }

        public void Leave(uint accountId)
        {
            if (accountId == _globalManager.Player.AccountId)
                Dismiss();
            else
            {
                if (CurrentParty != null)
                {
                    CurrentParty.Remove(accountId);
                    _globalManager.FMain.UpdateParty(CurrentParty.PartyMembers);
                }
            }
        }

        public void SetPartyMaster(uint worldId)
        {
            if (CurrentParty == null)
                return;

            CurrentParty.MasterId = worldId;
        }

        public bool ToggleInviteList(string charname)
        {
            if (charname == _globalManager.Player.Charname)
                return false;

            foreach (var player in AcceptInviteList.Where(player => player == charname))
            {
                AcceptInviteList.Remove(player);
                return false;
            }

            AcceptInviteList.Add(charname);
            return true;
        }

        public bool IsValidInvite(string charname) => AcceptInviteList.Any(player => player == charname);

        public bool IsPickableItem(uint owner)
        {
            if (CurrentParty == null)
                return false;

            return CurrentParty.PlayerInParty(owner) && CurrentParty.IsAutoShare();
        }

        protected override void MyThread()
        {
            while (BActive)
            {
                if (AutoInvite)
                {
                    foreach (var player in _globalManager.CharManager.PlayerList.Where(player => AcceptInviteList.Contains(player.Charname)).Where(player => (CurrentParty == null) || (CurrentParty?.PlayerInParty(player.Charname) == false)))
                        _globalManager.PacketManager.SendPartyInvite(player.WorldId, (byte)PartyType);
                }

                Thread.Sleep(5000);
            }
        }
    }
}
