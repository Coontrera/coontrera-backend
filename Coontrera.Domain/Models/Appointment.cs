namespace Coontrera.Domain.Models;

public class Appointment
{
    public string Id { get; private set; } = string.Empty;
    public User UserId {get; private set;} = User.Id;
    public ClinicService ClinicServiceId {get; private set;} = ClinicService.Id;
    public DateTime DateRegistered {get; private set;} = DateTime.UtcNow;
    public string TimeSlot
}