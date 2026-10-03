package com.example.TicketAndNotification.Models.Ticket;

import java.time.LocalDateTime;
import java.util.UUID;

import com.example.Shared.Enum.ETicketType;

import jakarta.persistence.Column;
import jakarta.persistence.Entity;
import jakarta.persistence.EnumType;
import jakarta.persistence.Enumerated;
import jakarta.persistence.Id;
import jakarta.persistence.Table;
import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;

@Entity 
@Table(name = "ticket")
@Getter 
@Setter 
@NoArgsConstructor 
@AllArgsConstructor 
@Builder 
public class TicketModel {
    @Id 
    @Column(name = "ticket_id", nullable = false)
    private UUID ticketId;

    @Column(name = "ticket_account_id", nullable = false)
    private UUID ticketAccountId;

    @Column(name = "ticket_content", nullable = false, length = 255)
    private String ticketContent;

    @Enumerated(EnumType.STRING)
    @Column(name = "ticket_type ", nullable = false)
    private ETicketType ticketType;

    @Column(name = "ticket_is_resolved", nullable = false)
    private Boolean ticketIsResolved;
 
    @Column(name = "ticket_created_at")
    private LocalDateTime ticketCreatedAt;
    
    @Column(name = "ticket_updated_at")
    private LocalDateTime ticketUpdatedAt;
}
