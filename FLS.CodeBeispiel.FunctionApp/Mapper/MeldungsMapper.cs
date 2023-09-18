namespace FLS.CodeBeispiel.FunctionApp.Mapper;

public static class MeldungsMapper
{
    public static Models.Domain.Meldung MapToDomainMeldung(this Models.External.Meldung externeMeldung)
    {
        return new Models.Domain.Meldung()
        {
            Partner = new Models.Domain.Partner()
            {
                Bezeichnung = externeMeldung.ExtPartner,
                AccountId = externeMeldung.ExtPartnerCrmId,
                Nummer = externeMeldung.ExtPartnerNr
            },
            Artikel = externeMeldung.Artikel.Select(art => new Models.Domain.Artikel()
            {
                Bezeichnung = art.Bezeichnung,
                IntNr = art.IntArtikelnummer,
                ExtArtikelnummer = art.ExtArtikelnummer,
                Gebinde = art.Gebinde,
                Hersteller = art.Hersteller,
                FlaschenEAN = art.FlaschenGTIN,
                GebindeEAN = art.GebindeGTIN,
                TeilmengenEAN = art.TeilmengenGTIN,
            }),
            Katalog = externeMeldung.Preisgruppe.Select(str => new Models.Domain.Katalog()
            {
                Bezeichnung = str.Bezeichnung,
                PreisGrNr = str.Nummer,
            }).ToList()
        };
    }
}
