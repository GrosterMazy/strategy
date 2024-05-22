using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BarracksWindowButton : MonoBehaviour
{
    BarracksController _barracksController;
    public void OnClicked() { _barracksController.Order(transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text); } 
    private void Start() { _barracksController = FindObjectOfType<BarracksController>(); }
}
