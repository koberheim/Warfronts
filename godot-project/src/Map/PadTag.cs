namespace FrontsOfWar.Map;

// Build pad tags (GDD §7.5). A pad may carry more than one in principle, but
// launch maps only ever assign one — kept as a single enum for M1 simplicity.
public enum PadTag
{
    Standard,
    Elevated,
    Enclosed,
    Coastal,
}
