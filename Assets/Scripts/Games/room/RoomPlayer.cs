using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoomPlayer : MonoBehaviour
{
    [SerializeField] private TextMeshPro nickNameText;
    
    [SerializeField] private SpriteRenderer playerSprite;
    [SerializeField] private Texture2D teamPaletteTex;
    [SerializeField] private Texture2D skinPaletteTex;


    public int RoomIndex { get; private set; }
    
    public bool HasComfirmed { get; private set; }
    
    public void SetRoomPlayer(int index, string nickname)
    {
        RoomIndex = index;
        nickNameText.text = nickname;
        nickNameText.transform.rotation = Quaternion.identity;
    }
    
    public void SetConfirmed(bool isComfirmed) {
        HasComfirmed = isComfirmed;
    }

    public void ApplyAppearance(string country, int skinColor=0)
    {
        if (playerSprite == null || teamPaletteTex == null || skinPaletteTex == null)
            return;
        Material mat = playerSprite.material;

        mat.SetTexture("_TeamPalette", teamPaletteTex);
        mat.SetVector("_TeamPalette_TexelSize", new Vector4(
            1f / teamPaletteTex.width,
            1f / teamPaletteTex.height,
            teamPaletteTex.width,
            teamPaletteTex.height
        ));

        mat.SetTexture("_SkinPalette", skinPaletteTex);
        mat.SetVector("_SkinPalette_TexelSize", new Vector4(
            1f / skinPaletteTex.width,
            1f / skinPaletteTex.height,
            skinPaletteTex.width,
            skinPaletteTex.height
        ));

        mat.SetInt("_SkinColor",
            Mathf.Clamp(skinColor, 0, skinPaletteTex.height - 1));

        var countries = DataLoader.Instance.GetCountries();
        int teamIndex = countries.IndexOf(country);
        teamIndex = Mathf.Clamp(teamIndex, 0, teamPaletteTex.height - 1);
        mat.SetInt("_TeamColor", teamIndex);
    }


}
