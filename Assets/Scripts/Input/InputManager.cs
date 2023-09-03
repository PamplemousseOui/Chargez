using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public delegate void SimpleEvent();
    public static event SimpleEvent OnAttackPress;
    public static event SimpleEvent OnAttackRelease;
    
    public static event SimpleEvent OnDashPress;
    public static event SimpleEvent OnDashRelease;
    
    public static event SimpleEvent OnPausePress;

    private bool m_leftInputPress = false;
    private bool m_rightInputPress = false;
    
    public static bool isLeft => instance.m_leftInputPress && !instance.m_rightInputPress;
    public static bool isRight => !instance.m_leftInputPress && instance.m_rightInputPress;
    
    public static InputManager instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }
    
    public void ReadPauseInput(InputAction.CallbackContext _context)
    {
        if (_context.performed)
        {
            OnPausePress?.Invoke();
        }
    }

    public void ReadLeftInput(InputAction.CallbackContext _context)
    {
        if (_context.performed)
        {
            m_leftInputPress = true;
        }
        else if (_context.canceled)
        {
            m_leftInputPress = false;
        }
    }
    
    public void ReadRightInput(InputAction.CallbackContext _context)
    {
        if (_context.performed)
        {
            m_rightInputPress = true;
        }
        else if (_context.canceled)
        {
            m_rightInputPress = false;
        }
    }
    
    public void ReadAttackInput(InputAction.CallbackContext _context)
    {
        if (_context.performed)
        {
            OnAttackPress?.Invoke();
        }
        else if (_context.canceled)
        {
            OnAttackRelease?.Invoke();
        }
    }
    
    public void ReadDashInput(InputAction.CallbackContext _context)
    {
        if (_context.performed)
        {
            OnDashPress?.Invoke();
        }
        else if (_context.canceled)
        {
            OnDashRelease?.Invoke();
        }
    }
    
}
