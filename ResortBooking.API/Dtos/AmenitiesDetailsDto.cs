namespace ResortBooking.API.Dtos
{
    public class AmenitiesDetailsDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description {  get; set; }
        public int VillaId {  get; set; }
        public string? VillaName {  get; set; }

    }
}
