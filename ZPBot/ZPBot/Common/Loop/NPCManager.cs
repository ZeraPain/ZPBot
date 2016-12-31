using System.Collections.Generic;
using System.Linq;

namespace ZPBot.Common.Loop
{
    public class NpcManager
    {
        private readonly List<Npc> _npcList;
        private readonly object _lock;

        public NpcManager()
        {
            _npcList = new List<Npc>();
            _lock = new object();
        }

        public void Add(Npc npc)
        {
            lock (_lock)
            {
                _npcList.Add(npc);
            }
        }

        public void Remove(uint worldId)
        {
            lock (_lock)
            {
                foreach (var npc in _npcList.Where(npc => npc.WorldId == worldId))
                {
                    _npcList.Remove(npc);
                    break;
                }
            }
        }

        public uint GetNpcid(uint worldId)
        {
            lock (_lock)
            {
                return (from npc in _npcList where npc.WorldId == worldId select npc.NpcId).FirstOrDefault();
            }
        }

        public uint GetWorldId(uint npcId)
        {
            lock (_lock)
            {
                foreach (var npc in _npcList.Where(npc => npc.NpcId == npcId))
                {
                    return npc.WorldId;
                }
            }

            return 0;
        }
    }
}
