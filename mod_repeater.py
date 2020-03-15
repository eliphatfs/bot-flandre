# -*- coding: utf-8 -*-
"""
Repeater Module
Created on Sun Sep  1 15:55:06 2019

@author: eliphat
"""
import time

import tg_connection


repeat_counter = {}


async def process_repeat(session, chat_id, comm):
    o_count = repeat_counter.get(comm, (0, 0))[0]
    tm = int(time.time())
    repeat_counter[comm] = o_count + 1, tm
    if repeat_counter[comm][0] >= 2:
        await tg_connection.send_msg(session, chat_id, comm)
        repeat_counter[comm] = (-2 ** 30), 2 ** 63
    todel = set()
    for comm in repeat_counter:
        _, tx = repeat_counter[comm]
        if time.time() - tx > 60:
            todel.add(comm)
    for d in todel:
        repeat_counter.pop(d)
