//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.4.4
//     from Assets/Scripts/Player/PlayerActions.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class @PlayerActions : IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @PlayerActions()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""PlayerActions"",
    ""maps"": [
        {
            ""name"": ""CraftingTest"",
            ""id"": ""027dddd2-08a3-4133-b275-c1862cbaaf7d"",
            ""actions"": [
                {
                    ""name"": ""Craft"",
                    ""type"": ""Button"",
                    ""id"": ""dd04eafd-db24-4f8f-b4a1-e3c06de7a457"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Inventory"",
                    ""type"": ""Button"",
                    ""id"": ""f534e349-7f96-4c64-a7e3-91ee3f7c6ef2"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Rotate Clockwise"",
                    ""type"": ""Button"",
                    ""id"": ""b0bf948b-e10b-4953-897e-3877e9773bcf"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Rotate Anti Clockwise"",
                    ""type"": ""Button"",
                    ""id"": ""907361e7-127a-4f05-b0cd-20383341fca3"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Use Tool"",
                    ""type"": ""Button"",
                    ""id"": ""b20ba3fb-68c6-45d5-8d27-f4e424457f73"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Forward"",
                    ""type"": ""Button"",
                    ""id"": ""a0215b2e-7321-4369-b4c3-54fa789f6be6"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Use Right Tool"",
                    ""type"": ""Button"",
                    ""id"": ""639770f7-e28b-48e4-8024-1deff51316b7"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Equip Tool"",
                    ""type"": ""Button"",
                    ""id"": ""9863c829-dd32-4fab-9fab-c982932576e1"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""e43aefbe-b885-4495-920d-02000b02bcfe"",
                    ""path"": ""<Keyboard>/e"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Test"",
                    ""action"": ""Craft"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""87a58f67-0f0b-482d-8b05-367239d9425c"",
                    ""path"": ""<Keyboard>/i"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Inventory"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""cc4c9a31-b5d3-4cd9-930e-74f24f2c1588"",
                    ""path"": ""<Keyboard>/x"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Rotate Clockwise"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""1f304852-4e81-4619-9ddb-35c21b7332fd"",
                    ""path"": ""<Keyboard>/z"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Rotate Anti Clockwise"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""594ddd39-adf9-4a03-b747-c0530ba2542e"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Use Tool"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""71aec895-62dd-4e9b-a2f1-c312dd5062e6"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Forward"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""25bd9dd1-0347-4c47-8c74-bf2b22419eeb"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Use Right Tool"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""3776cac3-f22b-4379-a16d-78489e549c35"",
                    ""path"": ""<Keyboard>/tab"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Equip Tool"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""Test"",
            ""bindingGroup"": ""Test"",
            ""devices"": []
        }
    ]
}");
        // CraftingTest
        m_CraftingTest = asset.FindActionMap("CraftingTest", throwIfNotFound: true);
        m_CraftingTest_Craft = m_CraftingTest.FindAction("Craft", throwIfNotFound: true);
        m_CraftingTest_Inventory = m_CraftingTest.FindAction("Inventory", throwIfNotFound: true);
        m_CraftingTest_RotateClockwise = m_CraftingTest.FindAction("Rotate Clockwise", throwIfNotFound: true);
        m_CraftingTest_RotateAntiClockwise = m_CraftingTest.FindAction("Rotate Anti Clockwise", throwIfNotFound: true);
        m_CraftingTest_UseTool = m_CraftingTest.FindAction("Use Tool", throwIfNotFound: true);
        m_CraftingTest_Forward = m_CraftingTest.FindAction("Forward", throwIfNotFound: true);
        m_CraftingTest_UseRightTool = m_CraftingTest.FindAction("Use Right Tool", throwIfNotFound: true);
        m_CraftingTest_EquipTool = m_CraftingTest.FindAction("Equip Tool", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }
    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }
    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // CraftingTest
    private readonly InputActionMap m_CraftingTest;
    private ICraftingTestActions m_CraftingTestActionsCallbackInterface;
    private readonly InputAction m_CraftingTest_Craft;
    private readonly InputAction m_CraftingTest_Inventory;
    private readonly InputAction m_CraftingTest_RotateClockwise;
    private readonly InputAction m_CraftingTest_RotateAntiClockwise;
    private readonly InputAction m_CraftingTest_UseTool;
    private readonly InputAction m_CraftingTest_Forward;
    private readonly InputAction m_CraftingTest_UseRightTool;
    private readonly InputAction m_CraftingTest_EquipTool;
    public struct CraftingTestActions
    {
        private @PlayerActions m_Wrapper;
        public CraftingTestActions(@PlayerActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @Craft => m_Wrapper.m_CraftingTest_Craft;
        public InputAction @Inventory => m_Wrapper.m_CraftingTest_Inventory;
        public InputAction @RotateClockwise => m_Wrapper.m_CraftingTest_RotateClockwise;
        public InputAction @RotateAntiClockwise => m_Wrapper.m_CraftingTest_RotateAntiClockwise;
        public InputAction @UseTool => m_Wrapper.m_CraftingTest_UseTool;
        public InputAction @Forward => m_Wrapper.m_CraftingTest_Forward;
        public InputAction @UseRightTool => m_Wrapper.m_CraftingTest_UseRightTool;
        public InputAction @EquipTool => m_Wrapper.m_CraftingTest_EquipTool;
        public InputActionMap Get() { return m_Wrapper.m_CraftingTest; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(CraftingTestActions set) { return set.Get(); }
        public void SetCallbacks(ICraftingTestActions instance)
        {
            if (m_Wrapper.m_CraftingTestActionsCallbackInterface != null)
            {
                @Craft.started -= m_Wrapper.m_CraftingTestActionsCallbackInterface.OnCraft;
                @Craft.performed -= m_Wrapper.m_CraftingTestActionsCallbackInterface.OnCraft;
                @Craft.canceled -= m_Wrapper.m_CraftingTestActionsCallbackInterface.OnCraft;
                @Inventory.started -= m_Wrapper.m_CraftingTestActionsCallbackInterface.OnInventory;
                @Inventory.performed -= m_Wrapper.m_CraftingTestActionsCallbackInterface.OnInventory;
                @Inventory.canceled -= m_Wrapper.m_CraftingTestActionsCallbackInterface.OnInventory;
                @RotateClockwise.started -= m_Wrapper.m_CraftingTestActionsCallbackInterface.OnRotateClockwise;
                @RotateClockwise.performed -= m_Wrapper.m_CraftingTestActionsCallbackInterface.OnRotateClockwise;
                @RotateClockwise.canceled -= m_Wrapper.m_CraftingTestActionsCallbackInterface.OnRotateClockwise;
                @RotateAntiClockwise.started -= m_Wrapper.m_CraftingTestActionsCallbackInterface.OnRotateAntiClockwise;
                @RotateAntiClockwise.performed -= m_Wrapper.m_CraftingTestActionsCallbackInterface.OnRotateAntiClockwise;
                @RotateAntiClockwise.canceled -= m_Wrapper.m_CraftingTestActionsCallbackInterface.OnRotateAntiClockwise;
                @UseTool.started -= m_Wrapper.m_CraftingTestActionsCallbackInterface.OnUseTool;
                @UseTool.performed -= m_Wrapper.m_CraftingTestActionsCallbackInterface.OnUseTool;
                @UseTool.canceled -= m_Wrapper.m_CraftingTestActionsCallbackInterface.OnUseTool;
                @Forward.started -= m_Wrapper.m_CraftingTestActionsCallbackInterface.OnForward;
                @Forward.performed -= m_Wrapper.m_CraftingTestActionsCallbackInterface.OnForward;
                @Forward.canceled -= m_Wrapper.m_CraftingTestActionsCallbackInterface.OnForward;
                @UseRightTool.started -= m_Wrapper.m_CraftingTestActionsCallbackInterface.OnUseRightTool;
                @UseRightTool.performed -= m_Wrapper.m_CraftingTestActionsCallbackInterface.OnUseRightTool;
                @UseRightTool.canceled -= m_Wrapper.m_CraftingTestActionsCallbackInterface.OnUseRightTool;
                @EquipTool.started -= m_Wrapper.m_CraftingTestActionsCallbackInterface.OnEquipTool;
                @EquipTool.performed -= m_Wrapper.m_CraftingTestActionsCallbackInterface.OnEquipTool;
                @EquipTool.canceled -= m_Wrapper.m_CraftingTestActionsCallbackInterface.OnEquipTool;
            }
            m_Wrapper.m_CraftingTestActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Craft.started += instance.OnCraft;
                @Craft.performed += instance.OnCraft;
                @Craft.canceled += instance.OnCraft;
                @Inventory.started += instance.OnInventory;
                @Inventory.performed += instance.OnInventory;
                @Inventory.canceled += instance.OnInventory;
                @RotateClockwise.started += instance.OnRotateClockwise;
                @RotateClockwise.performed += instance.OnRotateClockwise;
                @RotateClockwise.canceled += instance.OnRotateClockwise;
                @RotateAntiClockwise.started += instance.OnRotateAntiClockwise;
                @RotateAntiClockwise.performed += instance.OnRotateAntiClockwise;
                @RotateAntiClockwise.canceled += instance.OnRotateAntiClockwise;
                @UseTool.started += instance.OnUseTool;
                @UseTool.performed += instance.OnUseTool;
                @UseTool.canceled += instance.OnUseTool;
                @Forward.started += instance.OnForward;
                @Forward.performed += instance.OnForward;
                @Forward.canceled += instance.OnForward;
                @UseRightTool.started += instance.OnUseRightTool;
                @UseRightTool.performed += instance.OnUseRightTool;
                @UseRightTool.canceled += instance.OnUseRightTool;
                @EquipTool.started += instance.OnEquipTool;
                @EquipTool.performed += instance.OnEquipTool;
                @EquipTool.canceled += instance.OnEquipTool;
            }
        }
    }
    public CraftingTestActions @CraftingTest => new CraftingTestActions(this);
    private int m_TestSchemeIndex = -1;
    public InputControlScheme TestScheme
    {
        get
        {
            if (m_TestSchemeIndex == -1) m_TestSchemeIndex = asset.FindControlSchemeIndex("Test");
            return asset.controlSchemes[m_TestSchemeIndex];
        }
    }
    public interface ICraftingTestActions
    {
        void OnCraft(InputAction.CallbackContext context);
        void OnInventory(InputAction.CallbackContext context);
        void OnRotateClockwise(InputAction.CallbackContext context);
        void OnRotateAntiClockwise(InputAction.CallbackContext context);
        void OnUseTool(InputAction.CallbackContext context);
        void OnForward(InputAction.CallbackContext context);
        void OnUseRightTool(InputAction.CallbackContext context);
        void OnEquipTool(InputAction.CallbackContext context);
    }
}
