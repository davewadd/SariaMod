namespace SariaMod.Dusts
{
    public class Snow : BaseTemplateDust
    {
        protected override YBounceMode BounceMode => YBounceMode.ClampTo2;
    }
}