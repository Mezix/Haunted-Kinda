using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public GameObject _UIOfferingPrefab;
    //public List<UIOffering> UIOfferings;
    public Dictionary<UIOffering, int> _UIOfferings = new Dictionary<UIOffering, int>();
    private void Start()
    {
        Events.current.OfferingPickedUp += PickUp;
    }

    private void PickUp(Offering newOffering)
    {
        bool isNewOffering = true;
        UIOffering uiOffering = null;
        if(_UIOfferings.Count > 0)
        {
            foreach(UIOffering offering in _UIOfferings.Keys)
            {
                if (offering.CompareTag(newOffering.tag)) //if an offering with this tag already exists, skip it! and add +1 to text
                {
                    isNewOffering = false;
                    uiOffering = offering;
                }
            }
        }
        if(isNewOffering)
        {
            print("new offering");

            GameObject InstantiatedOffering = Instantiate(_UIOfferingPrefab);
            InstantiatedOffering.GetComponent<RectTransform>().anchoredPosition += new Vector2(100 * _UIOfferings.Count,0);
            uiOffering = InstantiatedOffering.GetComponent<UIOffering>();
            uiOffering._offeringImage.sprite = newOffering.GetComponent<SpriteRenderer>().sprite;
            uiOffering._text.text = "x1";
            InstantiatedOffering.tag = newOffering.tag;
            InstantiatedOffering.transform.SetParent(gameObject.transform, false);
            _UIOfferings.Add(InstantiatedOffering.GetComponent<UIOffering>(), 1);
        }
        else
        {
            if(uiOffering)
            {
                _UIOfferings[uiOffering] += 1;
                uiOffering._text.text = "x" + _UIOfferings[uiOffering];
                print("old :(");
            }
        }
    }
}
