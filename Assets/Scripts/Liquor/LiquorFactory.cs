using UnityEngine;

namespace DrinkAlcoholic
{
    public class LiquorFactory : MonoBehaviour
    {
        [SerializeField] LiquorData[] liquorTable = new LiquorData[3];

        public Liquor CreateRandom() => new Liquor(liquorTable[Random.Range(0, liquorTable.Length)]);
    }
}
