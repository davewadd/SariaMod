namespace SariaMod.Dusts
{
    public class Psychic : BaseTemplateDust
    {
        protected override YBounceMode BounceMode => YBounceMode.None;
        protected override bool NoLight => true;
        protected override float LightIntensity => .09f;
    }
}