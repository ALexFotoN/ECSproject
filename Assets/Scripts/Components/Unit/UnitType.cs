using Unity.Entities;

namespace RTS.Components
{
    public enum UnitTypeEnum
    {
        Swordsman,
        Archer
    }

    public struct UnitType : IComponentData
    {
        public UnitTypeEnum Value;
    }
}
