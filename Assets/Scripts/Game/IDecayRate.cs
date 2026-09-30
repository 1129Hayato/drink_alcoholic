namespace DrinkAlcoholic
{
    /// <summary>経過時間に応じた、ゲージの毎秒減少量。</summary>
    public interface IDecayRate
    {
        float Evaluate(float time);
    }
}
