# ============================================================
#  organize-views.ps1
#  Reorganiza Components/Pages y Components/Shared en subcarpetas
#  por dominio. Si el proyecto es un repo Git usa "git mv" para
#  preservar el historial; si no, hace Move-Item normal.
#
#  USO:
#    1. Cierra Visual Studio / Rider antes de ejecutar.
#    2. Abre PowerShell en la raíz de VetCitasWA y corre:
#         ./organize-views.ps1
#    3. Revisa el resultado y borra el script si no lo necesitas.
# ============================================================

$ErrorActionPreference = "Stop"

# Detecta si estamos dentro de un repo Git
$useGit = $false
try {
    git rev-parse --is-inside-work-tree 2>$null | Out-Null
    if ($LASTEXITCODE -eq 0) { $useGit = $true }
} catch { $useGit = $false }

Write-Host ""
Write-Host "===== Organizando vistas de VetCitasWA =====" -ForegroundColor Cyan
if ($useGit) {
    Write-Host "Repo Git detectado: usando 'git mv' (preserva historial)." -ForegroundColor Green
} else {
    Write-Host "No es repo Git: usando 'Move-Item'." -ForegroundColor Yellow
}
Write-Host ""

# Mapa: origen relativo -> carpeta destino relativa
#       cada archivo .razor y su companion .razor.css se mueven juntos.
$moves = @(
    # ===== Pages/Citas/ =====
    @{ Src = "Components\Pages\AgendaCitas.razor";        Dest = "Components\Pages\Citas\" },
    @{ Src = "Components\Pages\CitasHoy.razor";           Dest = "Components\Pages\Citas\" },
    @{ Src = "Components\Pages\MisHorarios.razor";        Dest = "Components\Pages\Citas\" },
    @{ Src = "Components\Pages\AtencionesMedicas.razor";  Dest = "Components\Pages\Citas\" },

    # ===== Pages/Atencion/ =====
    @{ Src = "Components\Pages\AtencionRegistro.razor";   Dest = "Components\Pages\Atencion\" },

    # ===== Pages/Clientes/ =====
    @{ Src = "Components\Pages\ClientesMascotas.razor";          Dest = "Components\Pages\Clientes\" },
    @{ Src = "Components\Pages\ClientesMascotasDetalle.razor";   Dest = "Components\Pages\Clientes\" },
    @{ Src = "Components\Pages\ClienteDetalle.razor";            Dest = "Components\Pages\Clientes\" },

    # ===== Pages/Servicios/ =====
    @{ Src = "Components\Pages\Servicios.razor";          Dest = "Components\Pages\Servicios\" },
    @{ Src = "Components\Pages\ServicioForm.razor";       Dest = "Components\Pages\Servicios\" },

    # ===== Pages/Admin/ (módulos solo de Administrador) =====
    @{ Src = "Components\Pages\GestionUsuariosRoles.razor";   Dest = "Components\Pages\Admin\" },
    @{ Src = "Components\Pages\GestionarHorarios.razor";      Dest = "Components\Pages\Admin\" },
    @{ Src = "Components\Pages\Reportes.razor";               Dest = "Components\Pages\Admin\" },

    # ===== Pages/Recordatorios/ =====
    @{ Src = "Components\Pages\Recordatorios.razor";      Dest = "Components\Pages\Recordatorios\" },

    # ===== Pages/Errors/ =====
    @{ Src = "Components\Pages\Error.razor";              Dest = "Components\Pages\Errors\" },
    @{ Src = "Components\Pages\NotFound.razor";           Dest = "Components\Pages\Errors\" },

    # ===== Shared/Modals/ =====
    @{ Src = "Components\Shared\ConfirmDeleteModal.razor";    Dest = "Components\Shared\Modals\" },
    @{ Src = "Components\Shared\ClienteFormModal.razor";      Dest = "Components\Shared\Modals\" },
    @{ Src = "Components\Shared\MascotaFormModal.razor";      Dest = "Components\Shared\Modals\" },
    @{ Src = "Components\Shared\UsuarioFormModal.razor";      Dest = "Components\Shared\Modals\" },
    @{ Src = "Components\Shared\RecordatorioFormModal.razor"; Dest = "Components\Shared\Modals\" },
    @{ Src = "Components\Shared\CancelarCitaModal.razor";     Dest = "Components\Shared\Modals\" },
    @{ Src = "Components\Shared\ReprogramarCitaModal.razor";  Dest = "Components\Shared\Modals\" },
    @{ Src = "Components\Shared\CambiarVetModal.razor";       Dest = "Components\Shared\Modals\" }
)

# Archivos boilerplate de la plantilla Blazor que ya no se usan
$toDelete = @(
    "Components\Pages\Counter.razor",
    "Components\Pages\Weather.razor"
)

function Move-File {
    param([string]$Src, [string]$Dest)

    if (-not (Test-Path $Src)) {
        Write-Host "  (skip)  $Src no existe" -ForegroundColor DarkGray
        return
    }

    if (-not (Test-Path $Dest)) {
        New-Item -ItemType Directory -Path $Dest -Force | Out-Null
    }

    $destFile = Join-Path $Dest (Split-Path $Src -Leaf)

    if ($useGit) {
        git mv -f $Src $destFile
    } else {
        Move-Item -Force $Src $destFile
    }

    Write-Host "  movido  $Src  ->  $destFile" -ForegroundColor Gray
}

# Mover archivos + su companion .razor.css
foreach ($m in $moves) {
    Move-File -Src $m.Src -Dest $m.Dest

    $cssSrc = [System.IO.Path]::ChangeExtension($m.Src, ".razor.css")
    if (Test-Path $cssSrc) {
        Move-File -Src $cssSrc -Dest $m.Dest
    }
}

# Borrar boilerplate
Write-Host ""
Write-Host "===== Limpiando archivos boilerplate =====" -ForegroundColor Cyan
foreach ($f in $toDelete) {
    if (Test-Path $f) {
        if ($useGit) {
            git rm -f $f | Out-Null
        } else {
            Remove-Item -Force $f
        }
        Write-Host "  borrado  $f" -ForegroundColor DarkGray
    } else {
        Write-Host "  (skip)   $f no existe" -ForegroundColor DarkGray
    }
}

# Limpiar archivo temporal que el asistente pudo dejar
$tempFile = "Components\Pages\_organize_test.txt"
if (Test-Path $tempFile) {
    if ($useGit) { git rm -f $tempFile 2>$null | Out-Null } else { Remove-Item -Force $tempFile }
    Write-Host "  borrado  $tempFile" -ForegroundColor DarkGray
}

Write-Host ""
Write-Host "===== Listo =====" -ForegroundColor Green
Write-Host "Verifica que el proyecto compile y luego puedes borrar este script:" -ForegroundColor Green
Write-Host "  Remove-Item ./organize-views.ps1" -ForegroundColor White
Write-Host ""
