using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContactViz : MonoBehaviour
{
    public Image leftFoot;
    public Image rightFoot;

    public Color onColor;
    public Color offColor;

    public void updateContacts(bool[] Contacts)
    {
        leftFoot.color = (Contacts[0] ? onColor : offColor);
        rightFoot.color = (Contacts[1] ? onColor : offColor);
    }

}
