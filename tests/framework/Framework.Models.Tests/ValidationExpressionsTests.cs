
/********************************************************************************
 * Copyright (c) 2024 Contributors to the Eclipse Foundation
 *
 * See the NOTICE file(s) distributed with this work for additional
 * information regarding copyright ownership.
 *
 * This program and the accompanying materials are made available under the
 * terms of the Apache License, Version 2.0 which is available at
 * https://www.apache.org/licenses/LICENSE-2.0.
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 *
 * SPDX-License-Identifier: Apache-2.0
 ********************************************************************************/

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Tests;

public class ValidationExpressionsTests
{
    [Theory]
    [InlineData("ValidCompanyName123", true)] // Valid company name
    [InlineData("Company Name", true)] // Valid with space
    [InlineData("Company$Name", true)] // Valid with special character
    [InlineData("Company\\Name", true)] // Valid with backslash
    [InlineData("Company/Name", true)] // Valid with forward slash
    [InlineData("Company<Name>", true)] // Valid with angle brackets
    [InlineData("Company Name!", true)] // Valid with exclamation mark
    [InlineData("Company@Name", true)] // Valid with @ symbol
    [InlineData("C", true)] // Minimum valid length
    [InlineData("7-ELEVEN INTERNATIONAL LLC", true)]
    [InlineData("Recht 24/7 Schröder Rechtsanwaltsgesellschaft mbH", true)]
    [InlineData("ACE 9 SPÓŁKA Z OGRANICZONĄ ODPOWIEDZIALNOŚCIĄ", true)]
    [InlineData("GAMMO Europe Korlátolt Felelősségű Társaság", true)]
    [InlineData("摩根亚太优势混合型证券投资基金 (QDII)", true)]
    [InlineData("삼성", true)]
    [InlineData("三", true)]
    [InlineData("Currency £$€¥¢", true)]
    [InlineData("Brackets []()", true)]
    [InlineData("Punctuation !?,.;:", true)]
    [InlineData("Double \"Quote\" Company S.A.", true)]
    [InlineData("Single 'Quote' Company LLC", true)]
    [InlineData("Special Characters ^&%#@*/_-\\", true)]
    [InlineData("German: ÄÖÜß", true)]
    [InlineData("+SEN Inc.", true)] // leading special character
    [InlineData("Danish: ÆØÅ", true)]
    [InlineData("Czech: ČĎŇŘŠŤŽŮů", true)]
    [InlineData("Estonian: ÄÖÜŠŽ", true)]
    [InlineData("Slovak: ĽĹŔŠŤŽ", true)]
    [InlineData("Polish: ĄĆĘŁŃÓŚŹŻ", true)]
    [InlineData("Hungarian: ÁÉÍÓÖŐÚÜŰ", true)]
    [InlineData("Romanian: ÂÎŞŢ", true)]
    [InlineData("Bulgarian: ЙЪЬ", true)]
    [InlineData("Greek: ΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣΤΥΦΧΨΩ", true)]
    [InlineData("Turkish: ÇĞİıÖŞÜ", true)]
    [InlineData("Arabic: ابتثجحخدذرزسشصضطظعغفقكلمنهوي", true)]
    [InlineData("Hebrew: שלום עולם", true)]
    [InlineData("Hindi: अआइईउऊऋएऐओऔकखगघङचछजझञटठडढणतथदधनपफबभमयरलवशषस", true)]
    [InlineData("Tamil: அஆஇஈஉஊஎஏஐஒஓஔகஙசஞடணதநனபமயரலவழளஷஸஹ", true)]
    [InlineData("Japanese: あいうえおかきくけこさしすせそたちつてとなにぬねのはひふへほまみむめもやゆよらりるれろわをん", true)]
    [InlineData("Chinese: 你好 世界", true)]
    [InlineData("Korean: 안녕하세요 세계", true)]
    [InlineData("Vietnamese: ăâắáấàằầảẳẩãẵẫạặậđêéếèềẻểẽễẹệíìỉĩịôơóốớòồờỏổởõỗỡọộợưúứùừủửũữụựýỳỷỹỵ", true)]
    [InlineData("Singapore: 你好 世界", true)]
    [InlineData("Thai: สวัสดีชาวโลก", true)]
    [InlineData("Bayerische Motoren Werke Aktiengesellschaft ", false)] // Ends with whitespace
    [InlineData(" Bayerische Motoren Werke Aktiengesellschaft", false)] // starts with whitespace
    [InlineData("Bayerische  Motoren Werke Aktiengesellschaft", false)] // double whitespace
    [InlineData(@"123456789012345678901234567890
                  123456789012345678901234567890
                  123456789012345678901234567890
                  123456789012345678901234567890
                  123456789012345678901234567890
                  12345678901234567890", false)] // Exceeds 160 characters
    [InlineData("", false)] // Empty string
    [InlineData(" ", false)] // Single space
    [InlineData("   ", false)] // Multiple spaces
    public void TestCompanyNameRegex(string companyName, bool expectedResult)
    {
        var result = companyName.IsValidCompanyName();
        Assert.Equal(expectedResult, result);
    }
}
