using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class UIInputFieldGroup : MonoBehaviour
{
    public List<UIInputField> inputFields;

    // Use this for initialization

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < inputFields.Count; i++)
        {
            InputField inputField = inputFields[i];

            if (inputField.gameObject.Equals(EventSystem.current.currentSelectedGameObject))
            {
                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || Input.GetKeyDown(KeyCode.RightShift))
                    {
                        inputField = GetReverseInputField(i);
                    }
                    else
                    {
                        inputField = GetCircularInputField(i);
                    }

                    if (inputField != null)
                    {
                        inputField.ActivateInputField();
                    }

                    break;
                }
            }
        }
    }

    void OnEnable()
    {
        for (int i = 0; i < inputFields.Count; i++)
        {
            inputFields[i].onValueChanged += OnValueChanged;
        }
    }

    void OnDisable()
    {
        for (int i = 0; i < inputFields.Count; i++)
        {
            inputFields[i].onValueChanged -= OnValueChanged;
        }
    }

    UIInputField GetReverseInputField(int index)
    {
        if (inputFields != null && inputFields.Count > 0)
        {
            UIInputField inputField = null;
            bool circulated = false;

            for (int i = index - 1; i >= -1; i--)
            {
                if (i != -1)
                {
                    if (!inputFields[i].gameObject.activeSelf || !inputFields[i].enabled || !inputFields[i].interactable)
                    {
                        continue;
                    }

                    inputField = inputFields[i];
                    break;
                }
                else
                {
                    if (!circulated)
                    {
                        i = inputFields.Count - 1;
                        inputField = inputFields[i];
                        circulated = true;
                        break;
                    }
                    else
                    {
                        inputField = null;
                        break;
                    }
                }
            }

            return inputField;
        }

        return null;
    }

    UIInputField GetCircularInputField(int index)
    {
        if (inputFields != null && inputFields.Count > 0)
        {
            UIInputField inputField = null;
            bool circulated = false;

            for (int i = index + 1; i <= inputFields.Count; i++)
            {
                if (i != inputFields.Count)
                {
                    if (!inputFields[i].gameObject.activeSelf || !inputFields[i].enabled || !inputFields[i].interactable)
                    {
                        continue;
                    }

                    inputField = inputFields[i];
                    break;
                }
                else
                {
                    if (!circulated)
                    {
                        //i = 0;
                        //inputField = inputFields[i];
                        UIInputField interactableInput = inputFields.Find(t => t.interactable == true);
                        if (interactableInput != null)
                        {
                            inputField = interactableInput;
                        }
                        else inputField = inputFields[0];//null이면 기존그대로

                        circulated = true;
                        break;
                    }
                    else
                    {
                        inputField = null;
                        break;
                    }
                }
            }

            return inputField;
        }

        return null;
    }

    void OnValueChanged(UIInputField inputField, string value)
    {
        int index = inputFields.IndexOf(inputField);

        if (int.Equals(index, -1) || int.Equals(index, inputFields.Count - 1))
        {
            return;
        }

        if (!int.Equals(inputField.characterLimit, 0) && int.Equals(inputField.characterLimit, value.Length))
        {
            inputField = GetCircularInputField(index);

            if (inputField != null)
            {
                inputField.ActivateInputField();
            }
        }
    }
}
