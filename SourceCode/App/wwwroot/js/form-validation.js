// Browser validators for our own validation attributes in static SSR forms (Migration step 3.5), registered with Blazor's
// client-side validation: each attribute emits a rule by name (IClientValidationRuleProvider), and the validator of that name
// runs here. The server still validates every post. Each validator mirrors its C# rule, applying first the correction made when
// saving, since the user is only told about what can't be corrected; App.Tests runs both against the same table of values.
(function (global) {
    'use strict';

    // As InputCorrectionExtensions.AsOrdinaryText (Data), for a single line.
    const replacements = {
        '‘': "'", '’': "'", '‚': "'", '‛': "'", '′': "'",
        '“': '"', '”': '"', '„': '"', '‟': '"', '″': '"',
        '‐': '-', '‑': '-', '‒': '-', '–': '-', '—': '-', '―': '-', '−': '-',
        '…': '...',
        '​': '', '‌': '', '‍': '', '⁠': '', '﻿': ''
    };
    const control = /[\u0000-\u001F\u007F-\u009F]/;
    const spaceSeparator = /\p{Zs}/u;

    function asOrdinaryText(text) {
        if (!text) return text;
        let result = '';
        const normalized = text.normalize('NFC');
        // By code point, explicitly: some engines iterate a string by text element, and "\r\n" is one.
        for (let i = 0; i < normalized.length; i++) {
            const code = normalized.codePointAt(i);
            if (code > 0xFFFF) i++;
            const c = String.fromCodePoint(code);
            if (c === '\r') continue;
            if (c === '\n' || c === '\t') result += ' ';
            else if (Object.prototype.hasOwnProperty.call(replacements, c)) result += replacements[c];
            else if (control.test(c)) continue;
            else result += spaceSeparator.test(c) ? ' ' : c;
        }
        return result.replace(/ {2,}/g, ' ').trim();
    }

    // As TextRules.IsOrdinaryText (App): only Latin letters, digits and some punctuation and symbols.
    const permittedPunctuationOrSymbols = ' (),.:;-+*!?%&§#±°²³/«»£€´\'"ºª';
    const latinRanges = [[0x0061, 0x007A], [0x0041, 0x005A], [0x00C0, 0x00FF], [0x0100, 0x0148], [0x014A, 0x01BF], [0x1E00, 0x1E9E]];

    function isOrdinaryText(text) {
        if (!text) return true;
        for (let i = 0; i < text.length; i++) {
            const c = text.charAt(i);
            const code = text.charCodeAt(i);
            if (latinRanges.some(range => code >= range[0] && code <= range[1])) continue;
            if (code >= 0x0030 && code <= 0x0039) continue;
            if (permittedPunctuationOrSymbols.indexOf(c) >= 0) continue;
            return false;
        }
        return true;
    }

    const validators = {
        // [OrdinaryText]: valid when the text, as corrected when saving, is ordinary text. Empty is valid: [Required] decides.
        ordinarytext: context => ({ success: !context.value || isOrdinaryText(asOrdinaryText(context.value)) })
    };

    global.modulesRegistryValidation = { asOrdinaryText, isOrdinaryText, validators };

    if (global.Blazor && global.Blazor.formValidation) {
        for (const name of Object.keys(validators)) global.Blazor.formValidation.addValidator(name, validators[name]);
    }
})(globalThis);