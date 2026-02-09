namespace TownOfUs.Interfaces;

public interface ILoyalCrewmate
{
    bool CanBeTraitor { get; }
    bool CanBeCrewpostor { get; }
    bool CanBeEgotist { get; }
    bool CanBeOtherEvil { get; }
}