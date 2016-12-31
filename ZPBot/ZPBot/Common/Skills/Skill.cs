
namespace ZPBot.Common.Skills
{
    public class Skill
    {
        public uint Id;
        public uint WorldId;

        public string Code;
        public string Name;

        public byte MasteryTopColumn;
        public byte MasteryLowColumn;
        public byte Row;
        public byte Column;

        public bool GroupSkill;
        public bool SpeedDrugSkill;
        public bool Active;

        public override string ToString() => Name ?? "Error";
    }
}
