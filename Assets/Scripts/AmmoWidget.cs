using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class AmmoWidget : MonoBehaviour
{
    public TMPro.TMP_Text ammoText;

    public void Refresh(int ammoCount) {
        ammoText.text = ammoCount.ToString();
    }
}
