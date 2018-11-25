using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemDisplay : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler
{
    public Item item;
    GeneralGameController generalGameController;

    public string itemName;
    Text nameText;
    Text descriptionText;
    public Text amountText;
    public Image itemSprite;
    public Image greenCircle;
    Text damageText;
    Text fireRateText;
    Text rangeText;
    Text healingText;
    public int itemAmount;

    public Button oldOccupiedCell;
    public Button TransferOldOccupiedCell;
    private bool FillingCircleIsStarted = false;
    private bool onDrag = false;
    private int onRightClick = 0;
    private int onLeftClick = 0;
    public bool FindEmptyCell = true;

    void Start ()
    {
        generalGameController = GameObject.FindGameObjectWithTag("GeneralGameController").GetComponent<GeneralGameController>();

        nameText = generalGameController.nameText;
        descriptionText = generalGameController.descriptionText;
        damageText = generalGameController.damageText;
        fireRateText = generalGameController.fireRateText;
        rangeText = generalGameController.rangeText;
        healingText = generalGameController.healingText;
        itemName = item.name;
        itemSprite.sprite = item.itemSprite;
        itemSprite.color = new Color(itemSprite.color.r, itemSprite.color.g, itemSprite.color.b, 0.7f);
        greenCircle.fillAmount = 0;
        if (itemAmount == 0) { itemAmount = item.amount; }

        GetComponent<Image>().sprite = item.itemSprite;
        ResetItem();
    }

    void FixedUpdate()
    {
        if (Input.GetMouseButtonUp(0))
        {
            if (!onDrag)
            {
                if (onLeftClick == 1)
                {
                    if (Vector2.Distance(transform.position, new Vector2(Input.mousePosition.x, Input.mousePosition.y)) <= 32f)
                    {
                        generalGameController.Panels.SetActive(true);
                        ResetItem();
                        onLeftClick = 0;

                        ResetCellsColors();
                        var cell = transform.parent.GetComponent<Button>();
                        var colors = cell.colors;
                        colors.disabledColor = new Color(0, 0.2f, 1);
                        colors.colorMultiplier = 1.2f;
                        cell.colors = colors;
                    }
                }
            }

            if (onDrag)
            {
                if (generalGameController.CellList != null)
                {
                    for (int i = 0; i < generalGameController.CellList.Count; i++)
                    {
                        if (Vector2.Distance(itemSprite.transform.position, new Vector2(generalGameController.CellList[i].transform.position.x, generalGameController.CellList[i].transform.position.y)) <= 40f)
                        {
                            ResetCellsColors();
                            generalGameController.Panels.SetActive(false);
                            if (generalGameController.CellList[i].interactable)
                            {
                                if (oldOccupiedCell != null)
                                {
                                    transform.SetParent(generalGameController.itemsUI);
                                    transform.position = generalGameController.CellList[i].transform.position;
                                    transform.SetParent(generalGameController.CellList[i].transform);
                                    oldOccupiedCell.interactable = true;
                                }
                                else
                                {
                                    transform.position = generalGameController.CellList[i].transform.position;
                                    transform.SetParent(generalGameController.CellList[i].transform);
                                }
                                oldOccupiedCell = generalGameController.CellList[i];
                                oldOccupiedCell.interactable = false;
                                itemSprite.transform.position = transform.position;
                                break;
                            }
                            else if (!generalGameController.CellList[i].interactable)
                            {
                                if (oldOccupiedCell != null)
                                {
                                    Transform oldItem = generalGameController.CellList[i].transform.GetChild(0);
                                    ItemDisplay itemDisplay = oldItem.GetComponent<ItemDisplay>();

                                    if (itemDisplay.itemName == itemName)
                                    {
                                        if (itemDisplay.gameObject != gameObject)
                                        {
                                            if (itemDisplay.itemAmount < itemDisplay.item.maxAmount)
                                            {
                                                if ((itemDisplay.itemAmount + itemAmount) <= itemDisplay.item.maxAmount)
                                                {
                                                    itemDisplay.itemAmount += itemAmount;
                                                    itemDisplay.ResetItem();
                                                    oldOccupiedCell.interactable = true;
                                                    Destroy(gameObject);
                                                    break;
                                                }
                                                else
                                                {
                                                    int newAmount = (itemDisplay.itemAmount + itemAmount) - itemDisplay.item.maxAmount;
                                                    itemDisplay.itemAmount = itemDisplay.item.maxAmount;
                                                    itemDisplay.ResetItem();

                                                    itemAmount = newAmount;
                                                    ResetItem();
                                                    itemSprite.transform.position = transform.position;
                                                }
                                            }
                                            else
                                            {
                                                itemSprite.transform.position = transform.position;
                                            }
                                        }
                                        else
                                        {
                                            itemSprite.transform.position = transform.position;
                                        }
                                        break;
                                    }
                                    else
                                    {
                                        oldItem.SetParent(generalGameController.itemsUI);
                                        oldItem.transform.position = oldOccupiedCell.transform.position;
                                        oldItem.SetParent(oldOccupiedCell.transform);
                                        itemDisplay.oldOccupiedCell = oldOccupiedCell;

                                        transform.SetParent(generalGameController.itemsUI);
                                        transform.position = generalGameController.CellList[i].transform.position;
                                        transform.SetParent(generalGameController.CellList[i].transform);
                                        oldOccupiedCell = generalGameController.CellList[i];
                                        oldOccupiedCell.interactable = false;
                                        itemSprite.transform.position = transform.position;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        if (Input.GetMouseButtonUp(1))
        {
            if (!onDrag)
            {
                if (onRightClick == 1)
                {
                    if (Vector2.Distance(transform.position, new Vector2(Input.mousePosition.x, Input.mousePosition.y)) <= 32f)
                    {
                        if (generalGameController.currentOpenedSomething != null)
                        {
                            if (!FillingCircleIsStarted)
                            {
                                StartCoroutine(FillGreenCircle());
                                FillingCircleIsStarted = true;
                            }
                            else
                            {
                                greenCircle.fillAmount = 0;
                            }
                            TransferOldOccupiedCell = oldOccupiedCell;
                            onRightClick = 0;
                        }
                    }
                }
            }

            if (onDrag)
            {
                if (generalGameController.CellList != null)
                {
                    for (int i = 0; i < generalGameController.CellList.Count; i++)
                    {
                        if (Vector2.Distance(itemSprite.transform.position, new Vector2(generalGameController.CellList[i].transform.position.x, generalGameController.CellList[i].transform.position.y)) <= 40f)
                        {
                            ResetCellsColors();
                            generalGameController.Panels.SetActive(false);
                            if (generalGameController.CellList[i].interactable)
                            {
                                if (itemAmount == 1)
                                {
                                    if (oldOccupiedCell != null)
                                    {
                                        transform.SetParent(generalGameController.itemsUI);
                                        transform.position = generalGameController.CellList[i].transform.position;
                                        transform.SetParent(generalGameController.CellList[i].transform);
                                        oldOccupiedCell.interactable = true;
                                    }
                                    else
                                    {
                                        transform.position = generalGameController.CellList[i].transform.position;
                                        transform.SetParent(generalGameController.CellList[i].transform);
                                    }
                                    oldOccupiedCell = generalGameController.CellList[i];
                                    oldOccupiedCell.interactable = false;
                                    itemSprite.transform.position = transform.position;
                                    break;
                                }
                                else if (itemAmount > 1)
                                {
                                    if (oldOccupiedCell != null)
                                    {
                                        itemSprite.transform.position = transform.position;
                                        itemAmount--;
                                        ResetItem();

                                        GameObject newItem = Instantiate(gameObject, generalGameController.CellList[i].transform);
                                        ItemDisplay itemDisplay = newItem.GetComponent<ItemDisplay>();
                                        newItem.transform.position = generalGameController.CellList[i].transform.position;
                                        itemDisplay.itemSprite.transform.position = newItem.transform.position;
                                        itemDisplay.itemAmount = 1;
                                        itemDisplay.oldOccupiedCell = generalGameController.CellList[i];
                                        itemDisplay.oldOccupiedCell.interactable = false;
                                        break;
                                    }
                                }
                            }
                            else if (!generalGameController.CellList[i].interactable)
                            {
                                if (oldOccupiedCell != null)
                                {
                                    Transform oldItem = generalGameController.CellList[i].transform.GetChild(0);
                                    ItemDisplay itemDisplay = oldItem.GetComponent<ItemDisplay>();

                                    if (itemDisplay.itemName == itemName)
                                    {
                                        if (itemDisplay.gameObject != gameObject)
                                        {
                                            if (itemAmount == 1)
                                            {
                                                if (itemDisplay.itemAmount < itemDisplay.item.maxAmount)
                                                {
                                                    if ((itemDisplay.itemAmount + 1) <= itemDisplay.item.maxAmount)
                                                    {
                                                        itemDisplay.itemAmount++;
                                                        itemDisplay.ResetItem();
                                                        oldOccupiedCell.interactable = true;
                                                        Destroy(gameObject);
                                                        break;
                                                    }
                                                }
                                                else
                                                {
                                                    itemSprite.transform.position = transform.position;
                                                }
                                            }
                                            else
                                            {
                                                if ((itemDisplay.itemAmount + 1) <= itemDisplay.item.maxAmount)
                                                {
                                                    itemDisplay.itemAmount++;
                                                    itemDisplay.ResetItem();
                                                    itemSprite.transform.position = transform.position;
                                                    itemAmount--;
                                                    ResetItem();
                                                    break;
                                                }
                                                else
                                                {
                                                    itemSprite.transform.position = transform.position;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            itemSprite.transform.position = transform.position;
                                        }
                                    }
                                    else
                                    {
                                        if (itemAmount == 1)
                                        {
                                            oldItem.SetParent(generalGameController.itemsUI);
                                            oldItem.transform.position = oldOccupiedCell.transform.position;
                                            oldItem.SetParent(oldOccupiedCell.transform);
                                            itemDisplay.oldOccupiedCell = oldOccupiedCell;

                                            transform.SetParent(generalGameController.itemsUI);
                                            transform.position = generalGameController.CellList[i].transform.position;
                                            transform.SetParent(generalGameController.CellList[i].transform);
                                            oldOccupiedCell = generalGameController.CellList[i];
                                            oldOccupiedCell.interactable = false;
                                            itemSprite.transform.position = transform.position;
                                            break;
                                        }
                                        else
                                        {
                                            itemSprite.transform.position = transform.position;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        if (Input.GetMouseButtonUp(2))
        {
            if (onDrag)
            {
                if (generalGameController.CellList != null)
                {
                    for (int i = 0; i < generalGameController.CellList.Count; i++)
                    {
                        if (Vector2.Distance(itemSprite.transform.position, new Vector2(generalGameController.CellList[i].transform.position.x, generalGameController.CellList[i].transform.position.y)) <= 40f)
                        {
                            ResetCellsColors();
                            generalGameController.Panels.SetActive(false);
                            if (generalGameController.CellList[i].interactable)
                            {
                                if (itemAmount == 1)
                                {
                                    if (oldOccupiedCell != null)
                                    {
                                        transform.SetParent(generalGameController.itemsUI);
                                        transform.position = generalGameController.CellList[i].transform.position;
                                        transform.SetParent(generalGameController.CellList[i].transform);
                                        oldOccupiedCell.interactable = true;
                                    }
                                    else
                                    {
                                        transform.position = generalGameController.CellList[i].transform.position;
                                        transform.SetParent(generalGameController.CellList[i].transform);
                                    }
                                    oldOccupiedCell = generalGameController.CellList[i];
                                    oldOccupiedCell.interactable = false;
                                    itemSprite.transform.position = transform.position;
                                    break;
                                }
                                else if (itemAmount > 1)
                                {
                                    if (oldOccupiedCell != null)
                                    {
                                        itemSprite.transform.position = transform.position;
                                        int newItemAmount = Mathf.CeilToInt(itemAmount / 2);

                                        itemAmount -= newItemAmount;
                                        ResetItem();

                                        GameObject newItem = Instantiate(gameObject, generalGameController.CellList[i].transform);
                                        ItemDisplay itemDisplay = newItem.GetComponent<ItemDisplay>();
                                        newItem.transform.position = generalGameController.CellList[i].transform.position;
                                        itemDisplay.itemSprite.transform.position = newItem.transform.position;
                                        itemDisplay.itemAmount = newItemAmount;
                                        itemDisplay.oldOccupiedCell = generalGameController.CellList[i];
                                        itemDisplay.oldOccupiedCell.interactable = false;
                                        break;
                                    }
                                }
                            }
                            else if (!generalGameController.CellList[i].interactable)
                            {
                                if (oldOccupiedCell != null)
                                {
                                    Transform oldItem = generalGameController.CellList[i].transform.GetChild(0);
                                    ItemDisplay itemDisplay = oldItem.GetComponent<ItemDisplay>();

                                    if (itemDisplay.itemName == itemName)
                                    {
                                        if (itemDisplay.gameObject != gameObject)
                                        {
                                            if (itemAmount == 1)
                                            {
                                                itemDisplay.itemAmount++;
                                                itemDisplay.ResetItem();
                                                oldOccupiedCell.interactable = true;
                                                Destroy(gameObject);
                                                break;
                                            }
                                            else
                                            {
                                                int newItemAmount = Mathf.CeilToInt(itemAmount / 2);
                                                itemAmount -= newItemAmount;
                                                itemDisplay.itemAmount += newItemAmount;
                                                itemDisplay.ResetItem();

                                                itemSprite.transform.position = transform.position;
                                                ResetItem();
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            itemSprite.transform.position = transform.position;
                                        }
                                    }
                                    else
                                    {
                                        if (itemAmount == 1)
                                        {
                                            oldItem.SetParent(generalGameController.itemsUI);
                                            oldItem.transform.position = oldOccupiedCell.transform.position;
                                            oldItem.SetParent(oldOccupiedCell.transform);
                                            itemDisplay.oldOccupiedCell = oldOccupiedCell;

                                            transform.SetParent(generalGameController.itemsUI);
                                            transform.position = generalGameController.CellList[i].transform.position;
                                            transform.SetParent(generalGameController.CellList[i].transform);
                                            oldOccupiedCell = generalGameController.CellList[i];
                                            oldOccupiedCell.interactable = false;
                                            itemSprite.transform.position = transform.position;
                                            break;
                                        }
                                        else
                                        {
                                            itemSprite.transform.position = transform.position;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    IEnumerator FillGreenCircle()
    {
        while (greenCircle.fillAmount < 1)
        {
            yield return null;
            if (TransferOldOccupiedCell != oldOccupiedCell)
            {
                greenCircle.fillAmount = 1;
            }
            else
            {
                greenCircle.fillAmount += Time.deltaTime;
            }
        }
        greenCircle.fillAmount = 0;
        if (TransferOldOccupiedCell == oldOccupiedCell) { TransferItem(); }
        FillingCircleIsStarted = false;
    }

    private void TransferItem()
    {
        if (generalGameController.currentOpenedSomething != null)
        {
            FindEmptyCell = true;
            Transform oldCell = transform.parent;

            if (oldCell.CompareTag("Inventory"))
            {
                for (int i = 0; i < generalGameController.CellList.Count; i++) //Find same item
                {
                    if (!generalGameController.CellList[i].CompareTag("Inventory"))
                    {
                        if (!generalGameController.CellList[i].interactable)
                        {
                            if (oldOccupiedCell != null)
                            {
                                Transform oldItem = generalGameController.CellList[i].transform.GetChild(0);
                                ItemDisplay itemDisplay = oldItem.GetComponent<ItemDisplay>();

                                if (itemDisplay.itemName == itemName)
                                {
                                    if (itemDisplay.gameObject != gameObject)
                                    {
                                        if (itemDisplay.itemAmount < itemDisplay.item.maxAmount)
                                        {
                                            if ((itemDisplay.itemAmount + itemAmount) <= itemDisplay.item.maxAmount)
                                            {
                                                itemDisplay.itemAmount += itemAmount;
                                                itemDisplay.ResetItem();
                                                oldOccupiedCell.interactable = true;
                                                Destroy(gameObject);
                                                FindEmptyCell = false;
                                                break;
                                            }
                                            else
                                            {
                                                int newAmount = (itemDisplay.itemAmount + itemAmount) - itemDisplay.item.maxAmount;
                                                itemDisplay.itemAmount = itemDisplay.item.maxAmount;
                                                itemDisplay.ResetItem();

                                                itemAmount = newAmount;
                                                ResetItem();

                                                for (int j = 0; j < generalGameController.CellList.Count; j++)
                                                {
                                                    if (generalGameController.CellList[i].interactable)
                                                    {
                                                        if (oldOccupiedCell != null)
                                                        {
                                                            transform.SetParent(generalGameController.itemsUI);
                                                            transform.position = generalGameController.CellList[i].transform.position;
                                                            transform.SetParent(generalGameController.CellList[i].transform);
                                                            oldOccupiedCell.interactable = true;
                                                            oldOccupiedCell = generalGameController.CellList[i];
                                                            oldOccupiedCell.interactable = false;
                                                            itemSprite.transform.position = transform.position;
                                                            FindEmptyCell = false;
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (FindEmptyCell)
                {
                    for (int i = 0; i < generalGameController.CellList.Count; i++) //Find empty cell
                    {
                        if (!generalGameController.CellList[i].CompareTag("Inventory"))
                        {
                            if (generalGameController.CellList[i].interactable)
                            {
                                if (oldOccupiedCell != null)
                                {
                                    transform.SetParent(generalGameController.itemsUI);
                                    transform.position = generalGameController.CellList[i].transform.position;
                                    transform.SetParent(generalGameController.CellList[i].transform);
                                    oldOccupiedCell.interactable = true;
                                }
                                else
                                {
                                    transform.position = generalGameController.CellList[i].transform.position;
                                    transform.SetParent(generalGameController.CellList[i].transform);
                                }
                                oldOccupiedCell = generalGameController.CellList[i];
                                oldOccupiedCell.interactable = false;
                                break;
                            }
                        }
                    }
                }
            }
            else if (oldCell.CompareTag("Chest"))
            {
                for (int i = 0; i < generalGameController.CellList.Count; i++)
                {
                    if (!generalGameController.CellList[i].CompareTag("Chest"))
                    {
                        if (!generalGameController.CellList[i].interactable)
                        {
                            if (oldOccupiedCell != null)
                            {
                                Transform oldItem = generalGameController.CellList[i].transform.GetChild(0);
                                ItemDisplay itemDisplay = oldItem.GetComponent<ItemDisplay>();

                                if (itemDisplay.itemName == itemName)
                                {
                                    if (itemDisplay.gameObject != gameObject)
                                    {
                                        if (itemDisplay.itemAmount < itemDisplay.item.maxAmount)
                                        {
                                            if ((itemDisplay.itemAmount + itemAmount) <= itemDisplay.item.maxAmount)
                                            {
                                                itemDisplay.itemAmount += itemAmount;
                                                itemDisplay.ResetItem();
                                                oldOccupiedCell.interactable = true;
                                                Destroy(gameObject);
                                                FindEmptyCell = false;
                                                break;
                                            }
                                            else
                                            {
                                                int newAmount = (itemDisplay.itemAmount + itemAmount) - itemDisplay.item.maxAmount;
                                                itemDisplay.itemAmount = itemDisplay.item.maxAmount;
                                                itemDisplay.ResetItem();

                                                itemAmount = newAmount;
                                                ResetItem();

                                                for (int j = 0; j < generalGameController.CellList.Count; j++)
                                                {
                                                    if (generalGameController.CellList[i].interactable)
                                                    {
                                                        if (oldOccupiedCell != null)
                                                        {
                                                            transform.SetParent(generalGameController.itemsUI);
                                                            transform.position = generalGameController.CellList[i].transform.position;
                                                            transform.SetParent(generalGameController.CellList[i].transform);
                                                            oldOccupiedCell.interactable = true;
                                                            oldOccupiedCell = generalGameController.CellList[i];
                                                            oldOccupiedCell.interactable = false;
                                                            itemSprite.transform.position = transform.position;
                                                            FindEmptyCell = false;
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (FindEmptyCell)
                {
                    for (int i = 0; i < generalGameController.CellList.Count; i++)
                    {
                        if (!generalGameController.CellList[i].CompareTag("Chest"))
                        {
                            if (generalGameController.CellList[i].interactable)
                            {
                                if (oldOccupiedCell != null)
                                {
                                    transform.SetParent(generalGameController.itemsUI);
                                    transform.position = generalGameController.CellList[i].transform.position;
                                    transform.SetParent(generalGameController.CellList[i].transform);
                                    oldOccupiedCell.interactable = true;
                                }
                                else
                                {
                                    transform.position = generalGameController.CellList[i].transform.position;
                                    transform.SetParent(generalGameController.CellList[i].transform);
                                }
                                oldOccupiedCell = generalGameController.CellList[i];
                                oldOccupiedCell.interactable = false;
                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!onDrag) { onDrag = true; }
        onLeftClick = 0;
        onRightClick = 0;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2))
        {
            //transform.position = eventData.position;
            itemSprite.transform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (onDrag) { onDrag = false; }

        itemSprite.transform.position = transform.position;
    }

    public void ResetItem()
    {
        nameText.text = item.name;
        descriptionText.text = item.description;

        healingText.enabled = false;
        amountText.enabled = false;
        damageText.enabled = false;
        fireRateText.enabled = false;
        rangeText.enabled = false;

        if (item.itemTypes == Item.ItemTypes.Food)
        {
            if (itemAmount > 1)
            {
                amountText.enabled = true;
            }
            else { amountText.enabled = false; }

            amountText.text = itemAmount.ToString();
            healingText.enabled = true;
            healingText.text = "Healing: " + item.healing.ToString();
        }
        else if (item.itemTypes == Item.ItemTypes.Resources)
        {
            if (itemAmount > 1)
            {
                amountText.enabled = true;
            }
            else { amountText.enabled = false; }

            amountText.text = itemAmount.ToString();
        }
        else if (item.itemTypes == Item.ItemTypes.Weapon)
        {
            amountText.enabled = true;
            damageText.enabled = true;
            fireRateText.enabled = true;
            rangeText.enabled = true;
            amountText.text = itemAmount.ToString();
            damageText.text = "Damage: " + item.damage.ToString();
            fireRateText.text = "FireRate: " + item.fireRate.ToString();
            rangeText.text = "Range: " + item.range.ToString();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            onLeftClick = 1;
        }

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            onRightClick = 1;
        }
    }

    public void ResetCellsColors()
    {
        for (int i = 0; i < generalGameController.CellList.Count; i++)
        {
            var cell = generalGameController.CellList[i];
            var colors = cell.colors;
            colors.disabledColor = new Color(1, 1, 1);
            colors.colorMultiplier = 1;
            cell.colors = colors;
        }
    }
}