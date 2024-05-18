using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class SelectedObjectInformationDrawerConntroller : MonoBehaviour
{
    public TextMeshProUGUI Name;
    public TextMeshProUGUI Health;
    public TextMeshProUGUI Armour;
    public TextMeshProUGUI DmgReduction;
    public TextMeshProUGUI Actions;
    public TextMeshProUGUI Steps;
    public TextMeshProUGUI AttackDmg;
    public TextMeshProUGUI AttackRange;
    public TextMeshProUGUI FoodConsumption;
    public GameObject spellButtonPrefab;
    public GameObject background;

    private List<GameObject> _spellButtons = new List<GameObject>();
    private float _backgroundWidth;
    private float _backgroundHeight;
    private Vector3 space = new Vector3(70, 0, 0);
    private SelectionController _selectionController;
    private Caster _caster;

    private void Awake()
    {
        InitComponentLinks();
    }
    private void OnEnable()
    {
        SelectCaster();
        DrawInformationPanel();
    }

    private void OnDisable()
    {
        foreach (GameObject spellButton in _spellButtons)
        {
            Destroy(spellButton);
        }
    }

    private void SelectCaster()
    {
        if (!_selectionController.isAnyUnitSelected)
        {
            _caster = null;
            return; 
        }
        if (_selectionController.selectedUnit.TryGetComponent<Caster>(out Caster selectedCaster))
        {
            _caster = selectedCaster;
        }
        else
        {
            _caster = null;
        }
    }

    private void DrawInformationPanel()
    {
        Name.SetText("");
        Health.SetText("");
        Armour.SetText("");
        Actions.SetText("");
        Steps.SetText("");
        AttackDmg.SetText("");
        AttackRange.SetText("");
        FoodConsumption.SetText("");
        if (_selectionController.isAnyFirstFactionFacilitySelected)
        {
            Name.SetText(_selectionController.selectedFacility.name);
            Health.SetText("Health: " + _selectionController.selectedFacility.GetComponent<FacilityHealth>().currentHealth.ToString() + " / " + _selectionController.selectedFacility.MaxHealth.ToString());
            Armour.SetText("Armour: " + _selectionController.selectedFacility.Armor + ", ");
            DmgReduction.SetText("Dmg reduction: " + _selectionController.selectedFacility.DamageReductionPercent + "%");
            Actions.SetText("Light consumption: " + _selectionController.selectedFacility.LightConsumption);
            Steps.SetText("Wood consumption: " + _selectionController.selectedFacility.WoodConsumption);
            AttackDmg.SetText("Food consumption: " + _selectionController.selectedFacility.FoodConsumption);
            AttackRange.SetText("Ore consumption: " + _selectionController.selectedFacility.OreConsumption);
        }
        else if (_selectionController.isAnyUnitSelected)
        {
            Name.SetText(_selectionController.selectedUnit.name);
            Health.SetText("Health: " + _selectionController.selectedUnit.GetComponent<UnitHealth>().currentHealth.ToString() + " / " + _selectionController.selectedUnit.Health.ToString());
            Armour.SetText("Armour: " + _selectionController.selectedUnit.Armor + ", ");
            DmgReduction.SetText("Dmg reduction: " + _selectionController.selectedUnit.DamageReductionPercent + "%");
            Actions.SetText("Actions: " + _selectionController.selectedUnit.GetComponent<UnitActions>().remainingActionsCount + " / " + _selectionController.selectedUnit.ActionsPerTurn);
            Steps.SetText("Steps: " + (_selectionController.selectedUnit.MovementSpeed - _selectionController.selectedUnit.GetComponent<UnitMovement>().spentSpeed) + " / " + _selectionController.selectedUnit.MovementSpeed);
            AttackDmg.SetText("Attack dmg: " + _selectionController.selectedUnit.AttackDamage);
            AttackRange.SetText("Attack range: " + _selectionController.selectedUnit.AttackRange);
            FoodConsumption.SetText("Food consumption: " + _selectionController.selectedUnit.FoodConsumption);
            if (_caster != null)
            {
                Vector3 offset = new Vector3(-_backgroundWidth / 2 + 45, -_backgroundHeight / 2 + 45, 0);
                foreach (SpellsDescription spell in _caster.SpellsList)
                {
                    if (spell.IsOnButton)
                    {
                        GameObject newSpellButton = Instantiate(spellButtonPrefab, background.transform);
                        newSpellButton.GetComponent<Button>().onClick.AddListener(() => _caster.PrepareSpell(spell, _caster.GetComponent<UnitDescription>()));
                        _spellButtons.Add(newSpellButton);
                        RectTransform newSpellButtonRect = newSpellButton.GetComponent<RectTransform>();
                        float newX = newSpellButtonRect.rect.position.x + offset.x;
                        float newY = newSpellButtonRect.rect.position.y + offset.y;
                        newSpellButtonRect.localPosition += offset;
                        offset += space;
                    }
                }
            }
        }
    }



    private void InitComponentLinks()
    {
        _selectionController = FindObjectOfType<SelectionController>();
        _backgroundWidth = background.GetComponent<RectTransform>().rect.width;
        _backgroundHeight = background.GetComponent<RectTransform>().rect.height;
    }
}
