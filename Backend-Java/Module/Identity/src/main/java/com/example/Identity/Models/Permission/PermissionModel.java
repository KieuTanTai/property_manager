package com.example.Identity.Models.Permission;

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
@Table(name = "permission")
@Getter 
@Setter 
@NoArgsConstructor 
@AllArgsConstructor 
@Builder 
public class PermissionModel {
    @Id 
    @Column(name = "permission_id", nullable=false)
    private UUID permissionId;

    @Column(name = "permission_code", nullable=false, length=50)
    private String permissionCode;

    @Column(name = "permission_name", nullable=false, length=150)
    private String permissionName;

    @Column(name = "permission_description", length=300)
    private String permissionDescription;

    @Column(name = "permission_is_active", nullable=false)
    private Boolean permissionIsActive;

    @Column(name = "permission_created_at")
    private LocalDateTime permissionCreatedAt;

    @Column(name = "permission_updated_at")
    private LocalDateTime permissionUpdatedAt;
}
