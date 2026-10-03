package com.example.contract.Models.Contract;

import java.math.BigDecimal;
import java.time.LocalDateTime;
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
@Table(name = "regulation")
@Getter 
@Setter 
@NoArgsConstructor 
@AllArgsConstructor 
@Builder 
public class RegulationModel {
    @Id 
    @Column(name = "regulation_id", nullable = false)
    private UUID regulationId;

    @Column(name = "regulation_name", nullable = false, length = 50)
    private String regulationName;

    @Column(name = "regulation_description", nullable = false, length = 255)
    private String regulationDescription;

    @Column(name = "regulation_fine_amount", precision = 18, scale = 2)
    private BigDecimal regulationFineAmount;

    @Column(name = "regulation_is_active", nullable = false)
    private Boolean regulationIsActive;

    @Column(name = "regulation_created_at")
    private LocalDateTime regulationCreatedAt;

    @Column(name = "regulation_updated_at")
    private LocalDateTime regulationUpdatedAt;
}
