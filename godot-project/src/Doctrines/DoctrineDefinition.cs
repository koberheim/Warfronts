using Godot;

namespace FrontsOfWar.Doctrines;

// One doctrine: a passive plus a fourth ability (GDD §8.3 — "18 doctrines,
// 3 per nation ... Each = one passive + one ability", §19 prompt 39). Pure
// data; DoctrineSystem is the only code that interprets it.
[GlobalClass]
public partial class DoctrineDefinition : Resource
{
    [Export] public string Id = "";
    [Export] public string NationId = "";
    [Export] public string DisplayName = "";

    // The ability's own short name (e.g. "Materiel Drop") — distinct from
    // DisplayName (the doctrine's name, e.g. "Lend-Lease"), needed by the
    // hotbar's doctrine slot for its button label.
    [Export] public string AbilityName = "";

    [Export] public string PassiveDescription = "";
    [Export] public string AbilityDescription = "";

    [Export] public DoctrinePassive Passive;
    [Export] public DoctrineAbility Ability;
}
