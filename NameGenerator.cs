public class NameGenerator
{
    public static string[] MaleNames = { 
        "Ahmose", "Ahmosis", "Amon", "Amun", "Amen", "Ammon", "Amun", "Appian", "Dewen",
        "Haaibre", "Hapi", "Hep", "Hap", "Hapy", "Kamose", "Khafra", "Khafre", "Menes", "Mysis",
        "Maahes", "Mahes", "Mihos", "Seti", "Seth",  "Amenemhat", "Amenemope", "Amenemopet", 
        "Amenhotep", "Amenirdis", "Amenmose", "Ameny", "Djehuty", "Harsiese", "Hori", "Huy", 
        "Khaemwaset", "Khenemetneferhedjet", "Kheti", "Khnumhotep", "Menkheperre", "Mentuherkhepeshef", 
        "Mentuhotep", "Mery", "Meryre", "Minmose", "Neferkare", "Nimlot", "Panehesy", "Pedubastis", 
        "Ptahhotep ", "Ptahmose", "Qar", "Ramesses", "Ramose", "Sekhemrekhutawy", "Senusret", 
        "Setepenre", "Shoshenq", "Siamun", "Smendes", "Sobekemsaf", "Sobekhotep", "Tentamun", 
        "Thutmose", "Tiye", "Siese", "Kawab", "Osorkon", "Ramesses"
    };

    public static string[] FemaleNames = { 
        "Ahmose", "Ahhotep", "Amenemope", "Amenemopet", "Amenirdis", "Ankhesenpepi", "Ankhesenpepy",
        "Ankhesenmeryre", "Henuttawy", "Henttawy", "Henuttaui", "Hetepheres", "Huy", "Iset", "Isetemkheb", 
        "Isetnofret", "Isetneferet", "Isisnofret", "Karomama", "Karamama", "Karomat", "Karoma", "Karoama",
        "Kamama", "Khamerernebty", "Khenemetneferhedjet", "Khentkaus", "Maatkare", "Mentuhotep",
        "Meresankh", "Meritamen", "Meritamun", "Merytamen", "Meryetamen", "Meritites", "Mery", "Meri",
        "Neferneferuaten", "Nefertari", "Neferu", "Setepenre", "Sitamun"
    };

    public static string Random(Person.GenderType gender)
    {
        switch (gender)
        {
            case Person.GenderType.MALE: 
                return MaleNames[Globals.Rand.Next(MaleNames.Length)];
            case Person.GenderType.FEMALE: 
                return FemaleNames[Globals.Rand.Next(FemaleNames.Length)];
        }
        return "ERROR";
    }
}