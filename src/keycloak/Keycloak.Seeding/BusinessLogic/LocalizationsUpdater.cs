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

using Org.Eclipse.TractusX.Portal.Backend.Framework.Linq;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Factory;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.BusinessLogic;

public class LocalizationsUpdater(IKeycloakFactory keycloakFactory, ISeedDataHandler seedDataHandler)
    : ILocalizationsUpdater
{
    public async Task UpdateLocalizations(string keycloakInstanceName, CancellationToken cancellationToken)
    {
        var keycloak = keycloakFactory.CreateKeycloakClient(keycloakInstanceName);
        var realm = seedDataHandler.Realm;
        var seederConfig = seedDataHandler.GetSpecificConfiguration(ConfigurationKey.Localizations);
        var localizations = await keycloak.GetLocaleAsync(realm, cancellationToken: cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        var updateRealmLocalizations = seedDataHandler.RealmLocalizations;

        await UpdateLocaleTranslations(keycloak, realm, localizations, updateRealmLocalizations, seederConfig, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        foreach (var deleteTranslation in localizations
                     .Where(x => seederConfig.ModificationAllowed(ModificationType.Delete, x))
                     .ExceptBy(updateRealmLocalizations.Select(t => t.Locale), locale => locale))
        {
            await keycloak.DeleteLocaleAsync(realm, deleteTranslation, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        }
    }

    private static async Task UpdateLocaleTranslations(KeycloakClient keycloak, string realm, IEnumerable<string> locales,
        IEnumerable<(string Locale, IEnumerable<KeyValuePair<string, string>> Translations)> translations,
        KeycloakSeederConfigModel seederConfig, CancellationToken cancellationToken)
    {
        if (!await locales
                .Join(
                    translations,
                    l => l,
                    trans => trans.Locale,
                    (l, trans) => (Locale: l, Update: trans))
                .IfAnyAwait(async localesToUpdate =>
                {
                    foreach (var (locale, update) in
                            localesToUpdate)
                    {
                        var localizations = await keycloak.GetLocaleAsync(realm, locale, cancellationToken: cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
                        await UpdateLocales(keycloak, realm, update, localizations, locale, seederConfig, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
                        await DeleteLocales(keycloak, realm, localizations, update, locale, seederConfig, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
                    }
                }).ConfigureAwait(false))
        {
            await AddLocales(keycloak, realm, translations, seederConfig, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        }
    }

    private static async Task DeleteLocales(KeycloakClient keycloak, string realm,
        IEnumerable<KeyValuePair<string, string>> localizations, (string Locale, IEnumerable<KeyValuePair<string, string>> Translations) update, string locale,
        KeycloakSeederConfigModel seederConfig, CancellationToken cancellationToken)
    {
        foreach (var deleteTranslation in localizations
                     .Where(x => seederConfig.ModificationAllowed(ModificationType.Delete, x.Key))
                     .ExceptBy(update.Translations.Select(t => t.Key), l => l.Key))
        {
            await keycloak.DeleteLocaleAsync(realm, locale, deleteTranslation.Key, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        }
    }

    private static async Task UpdateLocales(KeycloakClient keycloak, string realm,
        (string Locale, IEnumerable<KeyValuePair<string, string>> Translations) update, IEnumerable<KeyValuePair<string, string>> localizations, string locale,
        KeycloakSeederConfigModel seederConfig, CancellationToken cancellationToken)
    {
        foreach (var missingTranslation in update.Translations
                     .Where(x => seederConfig.ModificationAllowed(ModificationType.Update, x.Key))
                     .ExceptBy(localizations.Select(loc => loc.Key), locModel => locModel.Key))
        {
            await keycloak.UpdateLocaleAsync(realm, locale, missingTranslation.Key, missingTranslation.Value, cancellationToken).ConfigureAwait(false);
        }

        foreach (var updateTranslation in
                 localizations
                    .Where(x => seederConfig.ModificationAllowed(ModificationType.Update, x.Key))
                    .Join(
                        update.Translations,
                        l => l.Key,
                        trans => trans.Key,
                        (l, trans) => (Key: l.Key, Update: trans)))
        {
            await keycloak.UpdateLocaleAsync(realm, locale, updateTranslation.Key, updateTranslation.Update.Value, cancellationToken).ConfigureAwait(false);
        }
    }

    private static async Task AddLocales(KeycloakClient keycloak, string realm, IEnumerable<(string Locale, IEnumerable<KeyValuePair<string, string>> Translations)> translations,
        KeycloakSeederConfigModel seederConfig, CancellationToken cancellationToken)
    {
        foreach (var translation in translations.SelectMany(x => x.Translations.Select(t => (x.Locale, t.Key, t.Value))).Where(x => seederConfig.ModificationAllowed(ModificationType.Create, x.Key)))
        {
            await keycloak.UpdateLocaleAsync(realm, translation.Locale, translation.Key, translation.Value, cancellationToken).ConfigureAwait(false);
        }
    }
}
