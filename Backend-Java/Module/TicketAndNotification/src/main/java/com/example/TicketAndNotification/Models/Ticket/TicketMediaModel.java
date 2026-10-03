package com.example.TicketAndNotification.Models.Ticket;

import java.util.UUID;

import jakarta.persistence.Column;
import jakarta.persistence.Entity;
import jakarta.persistence.Id;
import jakarta.persistence.Table;
import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;

@Entity 
@Table(name = "ticket_media")
@Getter 
@Setter 
@NoArgsConstructor 
@AllArgsConstructor 
@Builder 
public class TicketMediaModel {
    @Id 
    @Column(name = "ticket_media_id", nullable = false)
    private UUID ticketMediaId;

    @Column(name = "ticket_media_ticket_id", nullable = false)
    private UUID ticketMediaticketId;

    @Column(name = "ticket_media_image_url", nullable = false, length = 255)
    private String ticketMediaImageUrl;
}
