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
        public Party Party { get; protected set; }

        private readonly GlobalManager _globalManager;

        public PartyManager(GlobalManager globalManager)
        {
            _globalManager = globalManager;
            Party = new Party(globalManager);
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

        public void JoinParty(uint partyId, uint masterId, byte partyType, [NotNull] List<PartyMember> partyMembers)
        {
            Party.Create(partyId, masterId, partyType, partyMembers);

            foreach (var partyMember in partyMembers.Where(partyMember => IsValidInvite(partyMember.Charname)))
                Party.SetAdditional(partyMember.Charname, "Acpt/Inv");
        }

        public void Dismiss() => Party.Clear();

        public void Join(PartyMember partyMember)
        {
            if (Party.Id == 0)
                return;

            Party.Add(partyMember);
            if (IsValidInvite(partyMember.Charname)) Party.SetAdditional(partyMember.Charname, "Acpt/Inv");
        }

        public void Leave(uint accountId)
        {
            if (accountId == _globalManager.Player.AccountId)
                Party.Clear();
            else
                Party.Remove(accountId);
        }

        public void SetPartyMaster(uint worldId) => Party.MasterId = worldId;

        public bool ToggleInviteList(string charname)
        {
            if (charname == _globalManager.Player.Charname)
                return false;

            foreach (var player in AcceptInviteList.Where(player => player == charname))
            {
                AcceptInviteList.Remove(player);
                Party.SetAdditional(charname, "");
                return false;
            }

            AcceptInviteList.Add(charname);
            Party.SetAdditional(charname, "Acpt/Inv");
            return true;
        }

        public bool IsValidInvite(string charname) => AcceptInviteList.Any(player => player == charname);

        public bool IsPickableItem(uint owner) => Party.PlayerInParty(owner) && Party.IsAutoShare();

        protected override void MyThread()
        {
            while (BActive)
            {
                if (AutoInvite)
                {
                    foreach (var player in AcceptInviteList.Select(charname => _globalManager.CharManager.GetCharIdByName(charname)).Where(player => player != null))
                    {
                        _globalManager.PacketManager.SendPartyInvite(player.WorldId, (byte)PartyType);
                    }
                }

                Thread.Sleep(5000);
            }
        }
    }
}
