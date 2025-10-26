using System.Linq;
using System.Collections.Generic;
using OpenUtau.Api;
using OpenUtau.Plugin.Builtin;

namespace IpeaPhonemizer
{
    [Phonemizer("IPÊ-A CVVC Phonemizer", "IPÊ-A CVVC", "ly", "PT")]
    public class IpeaCVVCPhonemizer : SyllableBasedPhonemizer
    {
        private readonly string[] vowels = { "a", "e", "i", "o", "u", "E", "O", "6", "am", "em", "im", "om", "um", "Ao" };
        /*private readonly string[] burstConsonants = { "b", "tch", "d", "dj", "g", "k", "p", "t" };
        private readonly string[] shortConsonants = { "r", "rr" };
        private readonly string[] longConsonants = { "s", "ch" };*/
        protected override List<string> ProcessSyllable(Syllable syllable)
        {
            var phonemes = new List<string>();
            var phoneme = string.Empty;

            var prevV = syllable.prevV;
            var cc = syllable.cc;
            var v = syllable.v;

            //nota começa com vogal
            if (syllable.IsStartingV)
            {
                phoneme = $"- {v}";
            }
            //nota precisa de transicao VV
            else if (syllable.IsVV)
            {
                //NAO adicionar nenhum fonema case seja um +
                if (!CanMakeAliasExtension(syllable))
                {
                    phoneme = $"{prevV} {v}";
                    //caso a transicao V V nao esteja disponivel, tente VV
                    if (!HasOto(phoneme, syllable.vowelTone))
                    {
                        phoneme = $"{prevV}{v}";
                        //caso a transicao VV nao esteja disponivel, tente alternativas
                        if (!HasOto(phoneme, syllable.vowelTone))
                        {
                            var alternative = string.Empty;
                            //para "dia", temos de usar [dj i][y a], ja que [i a] nao existe, etc
                            switch (prevV)
                            {
                                case "i":
                                    alternative = "y";
                                    break;
                                case "o":
                                case "u":
                                    alternative = "w";
                                    break;
                            }
                            phoneme = $"{alternative} {v}";
                            //se mesmo a transicao alternativa nao existir, usa so a vogal da nota, sem transicao
                            if (!HasOto(phoneme, syllable.vowelTone))
                            {
                                phoneme = v;
                            }
                        }
                    }
                }
                else
                {
                    phoneme = null;
                }
            }
            //notas no meio da frase
            else if (syllable.IsVCV)
            {
                //adiciona a transicao VC necessaria
                var VCV = $"{prevV} {cc[0]}";
                //caso nao exista a transicao, tente minusculo
                if (!HasOto(VCV, syllable.vowelTone))
                    VCV = $"{prevV} {cc[0].ToLower()}";
                //caso nao exista a transicao, tente juntar os fonemas
                if (!HasOto(VCV, syllable.vowelTone))
                    VCV = $"{prevV}{cc[0]}";
                //apenas adiciona a transicao caso exista
                if (HasOto(VCV, syllable.vowelTone))
                    phonemes.Add(VCV);
            }
            //nota começa com CV (ou ccv, tipo bra) ou no meio da frase
            if (syllable.IsStartingCV || syllable.IsVCV)
            {
                //adiciona todas as transicoes CC para todas as consoantes (menos a ultima), caso necessario
                for (int i = 0; i < cc.Length - 1; i++)
                {
                    var VC = $"{cc[i]} {cc[i + 1]}";
                    //checa se a transicao existe antes de adicionar
                    if (HasOto(VC, syllable.vowelTone))
                        phonemes.Add(VC);
                }
                //cria um CV juntando a ultima consoante e a vogal
                phoneme = $"{cc.Last()} {v}";
            }
            //por que botar o Add() aqui no final? por causa da bagunça do VV
            phonemes.Add(phoneme);
            return phonemes;
        }
        protected override List<string> ProcessEnding(Ending ending)
        {
            var phonemes = new List<string>();
            var phoneme = string.Empty;

            var prevV = ending.prevV;
            var cc = ending.cc;

            //checa se a nota termina com uma vogal
            if (ending.IsEndingV)
            {
                //coloca um end breath
                phoneme = $"{prevV} -";
            }
            //caso contrario, termina com consoante
            else
            {
                //coloca a primeira consoante do final da nota
                phoneme = $"{prevV} {cc[0]}";
                //caso nao exista a transicao, tente juntar os fonemas
                if (!HasOto(phoneme, ending.tone))
                {
                    phoneme = $"{prevV}{cc[0]}";
                }
            }

            phonemes.Add(phoneme);
            return phonemes;
        }
        protected override string[] GetVowels() => vowels;
    }
}