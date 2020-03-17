# -*- coding: utf-8 -*-
"""
Created on Tue Mar 17 11:55:27 2020

@author: eliphat
"""
import os
import pretty_midi
import scipy.io.wavfile as wav


def synth(midfile):
    midlib = pretty_midi.PrettyMIDI(midfile)
    waveform = midlib.synthesize(22050)[:22050 * 100]
    fn = "tmp/" + str(hash(midfile)) + ".wav"
    wav.write(fn, 22050, waveform)
    return fn


def do_search(fn):
    for c, ds, fs in os.walk("./midcollection"):
        for f in fs:
            if fn in f:
                return synth(os.path.join(c, f))
