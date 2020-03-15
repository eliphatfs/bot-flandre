# -*- coding: utf-8 -*-
"""
Created on Sun Mar 15 10:54:38 2020

@author: eliphat
"""
import pretty_midi
import random


major_pit = [0, 0, 2, 4, 5, 7, 9, 11]


def paste_note(note, velocity=None, pitch=None, start=None, end=None):
    return pretty_midi.Note(velocity or note.velocity,
                            pitch or note.pitch,
                            start or note.start,
                            end or note.end)


def find_note_down(pred, arr):
    for a in arr:
        if pred(a):
            return a
    return None


def march_seq(instr, base):
    begin = instr.notes[0]
    newnotes = list()
    scale = [(base or begin.pitch) - 24 + x for x in major_pit[1:]]
    scale += [(base or begin.pitch) - 12 + x for x in major_pit[1:]]
    scale += [(base or begin.pitch) + x for x in major_pit[1:]]
    harmo_p = (0, 7, 12)
    harmo_g = (3, 4, 8, 9)
    newnotes.append(paste_note(begin,
                               pitch=begin.pitch - random.choice(harmo_p[1:])))
    for i in range(1, len(instr.notes)):
        ldp = instr.notes[i - 1].pitch - newnotes[-1].pitch
        last_gen = newnotes[-1].pitch
        last_p = instr.notes[i - 1].pitch
        cur_n = instr.notes[i]
        cur_p = cur_n.pitch
        lala_gen = None if len(newnotes) < 2 else newnotes[-2].pitch

        def check_rand_add(pit, prob):
            if not pit:
                return False
            if pit >= cur_p:
                return False
            if abs(pit - cur_p) % 12 in harmo_p:
                if abs(ldp) % 12 in harmo_p:
                    return False  # Parallel
                if abs(cur_p - last_p) > 2:
                    if (cur_p - last_p) * (pit - last_gen) > 0:
                        return False  # Hidden
            elif abs(pit - cur_p) % 12 not in harmo_g:
                return False  # Not Harmonic
            if random.random() < prob:
                newnotes.append(paste_note(cur_n, pitch=pit))
                return True

        if cur_p == last_p:
            if random.random() < 0.5:
                newnotes.append(paste_note(cur_n, pitch=last_gen))
                continue
        nex = find_note_down(lambda x: x > last_gen, scale)
        pre = find_note_down(lambda x: x < last_gen, reversed(scale))
        if lala_gen and abs(lala_gen - last_gen) > 4:
            if last_gen > lala_gen:
                if check_rand_add(pre, 1.0):
                    continue
            elif check_rand_add(nex, 1.0):
                continue
        if check_rand_add(nex, 0.4):
            continue
        if check_rand_add(pre, 0.6667):
            continue
        for _ in range(30):
            if nex is not None:
                nex = find_note_down(lambda x: x > nex, scale)
            if pre is not None:
                pre = find_note_down(lambda x: x < pre, reversed(scale))
            if check_rand_add(nex, 0.25):
                break
            if check_rand_add(pre, 0.3333):
                break
            if cur_p == last_p:
                if random.random() < 0.5:
                    newnotes.append(paste_note(cur_n, pitch=last_gen))
                    continue
    for note in newnotes:
        instr.notes.append(note)
