using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using ZPBot.Annotations;
using ZPBot.Common.Resources;

namespace ZPBot.Common.Party
{
    internal class Party
    {
        public uint Id { get; protected set; }
        public uint MasterId { get; set; }
        public byte PartyType { get; protected set; }
        public BindingList<PartyMember> PartyMembers { get; protected set; }

        private readonly GlobalManager _globalManager;

        public Party(GlobalManager globalManager)
        {
            _globalManager = globalManager;
            Id = 0;
            MasterId = 0;
            PartyType = 0;
            PartyMembers = new BindingList<PartyMember>();
        }

        public void Clear()
        {
            if (_globalManager.FMain.InvokeRequired)
                _globalManager.FMain.Invoke((MethodInvoker) (Clear));
            else
            {
                Id = 0;
                MasterId = 0;
                PartyType = 0;
                PartyMembers.Clear();
            }
        }

        public void Create(uint id, uint masterId, byte partyType, [NotNull] List<PartyMember> partyMembers)
        {
            if (_globalManager.FMain.InvokeRequired)
                _globalManager.FMain.Invoke((MethodInvoker) (() => Create(id, masterId, partyType, partyMembers)));
            else
            {
                Id = id;
                MasterId = masterId;
                PartyType = partyType;

                lock (PartyMembers)
                {
                    PartyMembers.Clear();
                    foreach (var partyMember in partyMembers)
                        PartyMembers.Add(partyMember);
                }
            }
        }

        public void UpdateHpMp(uint accountId, byte hpMpInfo)
        {
            if (_globalManager.FMain.InvokeRequired)
                _globalManager.FMain.Invoke((MethodInvoker) (() => UpdateHpMp(accountId, hpMpInfo)));
            else
            {
                lock (PartyMembers)
                {
                    foreach (var partyMember in PartyMembers.Where(partyMember => partyMember.AccountId == accountId))
                        partyMember.HpMpInfo = hpMpInfo;
                }
            }
        }

        public void UpdatePosition(uint accountId, GamePosition inGamePosition)
        {
            if (_globalManager.FMain.InvokeRequired)
                _globalManager.FMain.Invoke((MethodInvoker) (() => UpdatePosition(accountId, inGamePosition)));
            else
            {
                lock (PartyMembers)
                {
                    foreach (var partyMember in PartyMembers.Where(partyMember => partyMember.AccountId == accountId))
                        partyMember.InGamePosition = inGamePosition;
                }

            }
        }

        public void Add(PartyMember partyMember)
        {
            if (_globalManager.FMain.InvokeRequired)
                _globalManager.FMain.Invoke((MethodInvoker) (() => Add(partyMember)));
            else
            {
                lock (PartyMembers)
                {
                    PartyMembers.Add(partyMember);
                }
            }
        }

        public void Remove(uint accountId)
        {
            if (_globalManager.FMain.InvokeRequired)
                _globalManager.FMain.Invoke((MethodInvoker) (() => Remove(accountId)));
            else
            {
                foreach (var partyMember in PartyMembers.Where(partyMember => partyMember.AccountId == accountId))
                {
                    PartyMembers.Remove(partyMember);
                    break;
                }
            }
        }

        public void SetAdditional(string charname, string additional)
        {
            if (_globalManager.FMain.InvokeRequired)
                _globalManager.FMain.Invoke((MethodInvoker) (() => SetAdditional(charname, additional)));
            else
            {
                foreach (var partyMember in PartyMembers.Where(partyMember => partyMember.Charname == charname))
                    partyMember.Additional = additional;
            }
        }

        public bool PlayerInParty(string charname) => PartyMembers.Any(player => player.Charname == charname);

        public bool PlayerInParty(uint accountId) => PartyMembers.Any(player => player.AccountId == accountId);

        public bool IsAutoShare() => PartyType % 4 == 2 || PartyType % 4 == 3;
    }
}
