# -*- coding: utf-8 -*-
"""
Flandre Gallery Module (/flandre)
Created on Sun Sep  1 15:36:31 2019

@author: eliphat
"""
import os
import random

import tg_connection


flanpic_dir = r"D:\AndroidProjects\ScarletKindom\image-downloader\images0825"
oss_root = 'https://scarletkindom.oss-cn-hangzhou.aliyuncs.com'
oss_fmt = oss_root + '/flandre_collection/%s/botproc'
flanpics = []
for cur_root, dirs, files in os.walk(flanpic_dir):
    for file in files:
        flanpics.append(file)


def internal_local_getpic():
    return os.path.join(flanpic_dir, random.choice(flanpics))


async def command_flandre(session, chat_id):
    photo_url = oss_fmt % random.choice(flanpics)
    await tg_connection.send_photo_by_url(session, chat_id, photo_url)
