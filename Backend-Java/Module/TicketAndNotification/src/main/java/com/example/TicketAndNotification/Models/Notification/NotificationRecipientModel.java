package com.example.TicketAndNotification.Models.Notification;

import java.io.Serializable;
import java.util.UUID;

import jakarta.persistence.Column;
import jakarta.persistence.Embeddable;
import jakarta.persistence.EmbeddedId;
import jakarta.persistence.Entity;
import jakarta.persistence.Table;
import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.EqualsAndHashCode;
import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;

@Entity 
@Table(name = "notification_recipient")
@Getter 
@Setter 
@AllArgsConstructor 
@NoArgsConstructor 
@Builder 
public class NotificationRecipientModel implements Serializable {

    @EmbeddedId 
    private NotificationRecipientModelId id;
    
    @Embeddable 
    @Getter 
    @Setter 
    @AllArgsConstructor 
    @NoArgsConstructor 
    @EqualsAndHashCode 
    public static class NotificationRecipientModelId implements Serializable {
        @Column(name = "nr_notification_id", nullable = false)
        private UUID notificationId;
        @Column(name = "nr_account_id", nullable = false)
        private String accountId;
    }
}
