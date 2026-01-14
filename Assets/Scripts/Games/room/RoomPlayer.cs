using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoomPlayer : MonoBehaviour
{
    [SerializeField] private TextMeshPro nickNameText;
    
    [SerializeField] private SpriteRenderer playerSprite;
    [SerializeField] private SpriteRenderer confirmSprite;
    [SerializeField] private Texture2D teamPaletteTex;
    [SerializeField] private Texture2D skinPaletteTex;

    [HideInInspector]public bool HasComfirmed=false;
    public int RoomIndex { get; private set; }
    private Animator animator;

    private void Awake() {
        animator=playerSprite.GetComponent<Animator>();
    }

    public void SetRoomPlayer(int index, string nickname)
    {
        RoomIndex = index;
        nickNameText.text = nickname;
        nickNameText.transform.rotation = Quaternion.identity;
        confirmSprite.transform.rotation = Quaternion.identity;
        confirmSprite.enabled = false;
        animator.Play("movement");
    }

    public void SetComfirmed(bool flag) {
        HasComfirmed = flag;
        confirmSprite.enabled = flag;
        if (flag) {
            animator.Play("walk");
        }
        else {
            animator.Play("movement");
        }
        
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
