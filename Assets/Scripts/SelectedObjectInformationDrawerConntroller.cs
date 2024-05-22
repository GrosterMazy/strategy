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
    public TextMeshProUGUI Regeneration;
    public TextMeshProUGUI Armour;
    public TextMeshProUGUI DmgReduction;
    public TextMeshProUGUI Actions;
    public TextMeshProUGUI Steps;
    public TextMeshProUGUI AttackDmg;
    public TextMeshProUGUI AttackRange;
    public TextMeshProUGUI FoodConsumption;
    public TextMeshProUGUI Fullness;
    public TextMeshProUGUI MiningModifier;
    public TextMeshProUGUI Inventory;
    public GameObject spellButtonPrefab;
    public GameObject background;

    private TurnManager _turnManager;
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
        TurnManager.onTurnChanged += SetTextColor;
        SetTextColor();
        SelectCaster();
        DrawInformationPanel();
    }

    private void OnDisable()
    {
        TurnManager.onTurnChanged -= SetTextColor;
        SetTextColor();
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
        Regeneration.SetText("");
        Armour.SetText("");
        DmgReduction.SetText("");
        Actions.SetText("");
        Steps.SetText("");
        AttackDmg.SetText("");
        AttackRange.SetText("");
        FoodConsumption.SetText("");
        Fullness.SetText("");
        MiningModifier.SetText("");
        Inventory.SetText("");
        if (_selectionController.isAnyFirstFactionFacilitySelected)
        {
            Name.SetText(_selectionController.selectedFacility.name);
            FacilityHealth facilityHealth = _selectionController.selectedFacility.GetComponent<FacilityHealth>();
            Health.SetText("Health: " + facilityHealth.currentHealth.ToString() + " / " + _selectionController.selectedFacility.MaxHealth.ToString());
            Regeneration.SetText("Repairment " + facilityHealth.HealthPerRepairment);
            Armour.SetText("Armour: " + _selectionController.selectedFacility.Armor);
            DmgReduction.SetText("Dmg reduction: " + _selectionController.selectedFacility.DamageReductionPercent + "%");
            Actions.SetText("Light consumption: " + _selectionController.selectedFacility.LightConsumption);
            Steps.SetText("Wood consumption: " + _selectionController.selectedFacility.WoodConsumption);
            AttackDmg.SetText("Food consumption: " + _selectionController.selectedFacility.FoodConsumption);
            AttackRange.SetText("Steel consumption: " + _selectionController.selectedFacility.SteelConsumption);
            string InventoryStr = "";
            foreach (string key in _selectionController.selectedFacility.Storage.Keys)
            {
                InventoryStr += key + ": " + _selectionController.selectedFacility.Storage[key] + "; ";
            }
            FoodConsumption.SetText("Inventory: " + "\n" + "      " + InventoryStr);
        }
        else if (_selectionController.isAnyUnitSelected && _selectionController.selectedUnit.GetComponent<DarknessUnitAI>() == null)
        {
            Name.SetText(_selectionController.selectedUnit.name);
            UnitHealth unitHealth = _selectionController.selectedUnit.GetComponent<UnitHealth>();
            Health.SetText("Health: " + unitHealth.currentHealth.ToString() + " / " + _selectionController.selectedUnit.Health.ToString());
            Regeneration.SetText("Regeneration " + unitHealth.regenerationPercent + " %");
            Armour.SetText("Armour: " + _selectionController.selectedUnit.Armor);
            DmgReduction.SetText("Dmg reduction: " + unitHealth.damageReductionPercent + "%");
            Actions.SetText("Actions: " + _selectionController.selectedUnit.GetComponent<UnitActions>().remainingActionsCount + " / " + _selectionController.selectedUnit.ActionsPerTurn);
            Steps.SetText("Steps: " + (_selectionController.selectedUnit.MovementSpeed - _selectionController.selectedUnit.GetComponent<UnitMovement>().spentSpeed) + " / " + _selectionController.selectedUnit.MovementSpeed);
            AttackDmg.SetText("Attack dmg: " + _selectionController.selectedUnit.AttackDamage);
            AttackRange.SetText("Attack range: " + _selectionController.selectedUnit.AttackRange);
            FoodConsumption.SetText("Food consumption: " + _selectionController.selectedUnit.FoodConsumption);
            if (_selectionController.selectedUnit.TryGetComponent<WorkerUnit>(out WorkerUnit unit)) 
            {
                Fullness.SetText("Fullness: " + (unit.WeightCapacityMax - unit._weightCapacityRemaining) + " / " + unit.WeightCapacityMax);
                MiningModifier.SetText("Mining modifier: " + unit.miningModifier);
                string InventoryStr = "";
                foreach (string key in unit.Inventory.Keys)
                {
                    InventoryStr += key + ": " + unit.Inventory[key] + "; ";
                }
                Inventory.SetText("Inventory: " + "\n" + "      " + InventoryStr);
            }
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
        else if (_selectionController.isAnyUnitSelected && _selectionController.selectedUnit.GetComponent<DarknessUnitAI>() != null)
        {
            Name.SetText(_selectionController.selectedUnit.name);
            DarknessUnitHealth unitHealth = _selectionController.selectedUnit.GetComponent<DarknessUnitHealth>();
            Health.SetText("Health: " + unitHealth.currentHealth.ToString() + " / " + _selectionController.selectedUnit.Health.ToString());
            Regeneration.SetText("Regeneration " + unitHealth.regeneration);
            Armour.SetText("Armour: " + _selectionController.selectedUnit.Armor);
            DmgReduction.SetText("Dmg reduction: " + _selectionController.selectedUnit.DamageReductionPercent + "%");
            Actions.SetText("Actions: " + _selectionController.selectedUnit.ActionsPerTurn);
            Steps.SetText("Steps: " + _selectionController.selectedUnit.MovementSpeed);
            AttackDmg.SetText("Attack dmg: " + _selectionController.selectedUnit.AttackDamage);
            AttackRange.SetText("Attack range: " + _selectionController.selectedUnit.AttackRange);
            FoodConsumption.SetText("Damage on the light: " + unitHealth.damageOnLight);
        }
        else if (_selectionController.isAnyToughResourceSelected)
        {
            Name.SetText(_selectionController.selectedToughResource.name);
            Health.SetText("Durability: " + _selectionController.selectedToughResource.remainingActionsToBreak);
        }
        else if (_selectionController.isAnyCollectableItemSelected)
        {
            Name.SetText(_selectionController.selectedCollectableItem.name);
        }
    }

    private void SetTextColor()
    {
        if (_turnManager.isDay)
        {
            Name.color = Color.black;
            Health.color = Color.black;
            Regeneration.color = Color.black;
            Armour.color = Color.black;
            DmgReduction.color = Color.black;
            Actions.color = Color.black;
            Steps.color = Color.black;
            AttackDmg.color = Color.black;
            AttackRange.color = Color.black;
            FoodConsumption.color = Color.black;
            Fullness.color = Color.black;
            MiningModifier.color = Color.black;
            Inventory.color = Color.black;
        }
        else
        {
            Name.color = Color.white;
            Health.color = Color.white;
            Regeneration.color = Color.white;
            Armour.color = Color.white;
            DmgReduction.color = Color.white;
            Actions.color = Color.white;
            Steps.color = Color.white;
            AttackDmg.color = Color.white;
            AttackRange.color = Color.white;
            FoodConsumption.color = Color.white;
            Fullness.color = Color.white;
            MiningModifier.color = Color.white;
            Inventory.color = Color.white;
        }
    }

    private void InitComponentLinks()
    {
        _turnManager = FindObjectOfType<TurnManager>();
        _selectionController = FindObjectOfType<SelectionController>();
        _backgroundWidth = background.GetComponent<RectTransform>().rect.width;
        _backgroundHeight = background.GetComponent<RectTransform>().rect.height;
    }
}
