using TMPro;
using UnityEngine;

public class CardView : MonoBehaviour
{
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text description;
    [SerializeField] private TMP_Text mana;
    [SerializeField] private SpriteRenderer imageSR;
    [SerializeField] private GameObject wrapper;

    public Card Card {  get; private set; }

    public void Setup(Card card)
    {
        Card = card;
        title.text = card.title;
        description.text = card.description;
        mana.text = card.mana.ToString();
        imageSR.sprite = card.image;
    }
    void OnMouseEnter()
    {
        wrapper.SetActive(false);
        Vector3 pos = new(transform.position.x, -2, 0 );
        CardViewHoverSystem.Instance.Show(Card, pos);
    }
    void OnMouseExit()
    {
        CardViewHoverSystem.Instance.Hide();
        wrapper.SetActive(true);

        
    }
}
