namespace SariaMod.Dusts
{
    public class Snow2 : BaseTemplateDust
    {
        protected override float ScaleMultiplier => .7f;
        protected override YBounceMode BounceMode => YBounceMode.ClampToHalf;
    }
}