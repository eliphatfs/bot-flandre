# -*- coding: utf-8 -*-
"""
Created on Sat Mar 14 13:39:10 2020

@author: eliphat
"""
import uuid
import pretty_midi
import scipy.io.wavfile as wav
import argparse
import traceback
import ext_music_automarch


major_pit = [0, 0, 2, 4, 5, 7, 9, 11]
modi = {
    ' ': 0,
    'b': -1,
    '#': 1,
    '+': 12,
    '-': -12
}


def note_time(bpm, beat):
    return beat * 60.0 / bpm


def pitch_mode(pitch, mode, base):
    if mode == 'note':
        if str.isdigit(pitch):
            return int(pitch)
        else:
            return pretty_midi.note_name_to_number(pitch)
    if pitch[0] == '0':
        return 0
    ret = base + major_pit[int(pitch[0])]
    for ch in pitch[1:]:
        ret += modi[ch]
    return ret


class ArgumentParser(argparse.ArgumentParser):

    def error(self, message):
        pass


def parse_args(input_str):
    parser = ArgumentParser(prog="flandre-music-synth")
    parser.add_argument("notes", type=str, nargs="*")
    parser.add_argument("--bpm", type=int, default=120)
    parser.add_argument("--mapmode", type=str, default='note')
    parser.add_argument("--base", type=str, default='C5')
    parser.add_argument("--automarch", type=int, default=0)
    parser.add_argument("--marchbase", type=str)
    args = parser.parse_args(input_str.split())
    return args


def parse_length(lenstr):
    first = ''
    modifiers = []
    for ch in lenstr:
        if ch >= '0' and ch <= '9':
            first += ch
        else:
            modifiers.append(ch)
    base_len = int(first)
    multi = 1.0
    for modifier in modifiers:
        if modifier == '_':
            multi *= 1.5
    return base_len / multi


def parse_lenorflag(nstr):
    if nstr[0] in 'kr':
        return 2, nstr[0]
    else:
        return 1, parse_length(nstr)


def parse_note(notestr, mapmode, base, last_len):
    data = notestr.split('.')
    pitch = pitch_mode(data[0], mapmode, base)
    retdata = [pitch, last_len, None]
    for d in data[1:]:
        idx, val = parse_lenorflag(d)
        retdata[idx] = val
    return retdata


def do_music(arg):
    try:
        args = parse_args(arg)

        if args.mapmode not in ('note', 'numbered'):
            return None
        base = pretty_midi.note_name_to_number(args.base)
        mapmode = args.mapmode
        marchbase = None
        if args.marchbase:
            marchbase = pretty_midi.note_name_to_number(args.marchbase)

        pmf = pretty_midi.PrettyMIDI(initial_tempo=args.bpm)
        pmf.instruments.append(pretty_midi.Instrument(0, name='main'))
        instr = pmf.instruments[-1]

        cur_time = 0.2
        last_len = 0.0
        for note in args.notes:
            pitch, length, flag = parse_note(note, mapmode, base, last_len)
            last_len = length
            length = note_time(args.bpm, 4.0 / length)
            if flag in (None, 'k', 'r'):
                note = pretty_midi.Note(100, pitch,
                                        cur_time, cur_time + length)
                if flag is None:
                    cur_time += length
                if flag == 'r':
                    cur_time = 0.2
                instr.notes.append(note)
        if args.automarch > 0:
            ext_music_automarch.march_seq(instr, marchbase)

        fn = "tmp/" + str(uuid.uuid4()) + ".wav"
        wav.write(fn, 22050, pmf.synthesize(fs=22050))
        return 'f', fn
    except Exception as exc:
        traceback.print_exc()
        return None
