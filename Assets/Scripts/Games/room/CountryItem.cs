using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CountryItem : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
{
    [SerializeField] private Image flagImage;
    public int Index;
    public string CountryName;
    

    public void Bind(CountryDataSo data)
    {   
        Index=data.countryId;
        CountryName = data.countryName;
        flagImage.sprite = data.flagSprite;
    }

    public void OnPointerClick(PointerEventData eventData) {
        RoomUI.Instance.SetCountryFromClick(CountryName);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        RoomUI.Instance.SetIndexFromMouse(Index);
    }
}
