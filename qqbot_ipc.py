# -*- coding: utf-8 -*-
"""
Created on Wed Feb 26 16:52:29 2020

@author: eliphat
"""
import win32api
import win32con
import win32gui
import time


hwnd = win32gui.FindWindow("TXGuiFoundation", "bot/ctf交流")
print(hwnd)


def send_msg(s):
    for c in s:
        win32api.SendMessage(hwnd, win32con.WM_CHAR, ord(c), 0)
        time.sleep(0.02)
    print(s)
    win32api.SendMessage(hwnd, win32con.WM_KEYDOWN, win32con.VK_RETURN, 0)
    win32api.SendMessage(hwnd, win32con.WM_KEYUP, win32con.VK_RETURN, 0)
    print(s)


# send_msg("强行中文")
