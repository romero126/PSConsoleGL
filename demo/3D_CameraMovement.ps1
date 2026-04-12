try {
    # PSConsoleGL 3D camera movement demo
    # Demonstrates Orbit, Zoom and Pan operations on Camera while rendering a colored cube

    $assembly = Join-Path $PSScriptRoot '..\src\PSConsoleGL\bin\Debug\netstandard2.0\PSConsoleGL.dll'
    if (-not (Test-Path $assembly)) {
        Write-Error "DLL not found at $assembly. Build the project first with ``dotnet build src/PSConsoleGL/PSConsoleGL.csproj``"
        exit 1
    }

    Add-Type -Path (Resolve-Path $assembly)
    # Enter alternate screen buffer and clear once
    $ansiEsc = [char]0x1b
    [Console]::Write("$ansiEsc[?1049h")
    $fb = [PSConsoleGL.Terminal.Drawing.FrameBuffer]::new()
    $aspect = [double]$fb.Width / [double]$fb.Height
    $cam = [PSConsoleGL.Terminal.Drawing.Camera]::new([PSConsoleGL.Terminal.Drawing.Vector3]::new(3,1.5,3), [PSConsoleGL.Terminal.Drawing.Vector3]::new(0,0,0), $aspect)
    $faceColors = [int[]]@(0xFFFF0000,0xFF00FF00,0xFF0000FF,0xFFFFFF00,0xFFFF00FF,0xFF00FFFF)
    $mesh = [PSConsoleGL.Terminal.Drawing.Mesh]::CreateUnitCube($faceColors)

    # Sequence: orbit for 72 frames, zoom in, pan right/up, zoom out
    # Orbit
    #for ($i = 0; $i -lt 72; $i++) {
    #    $fb.Clear(0,0,0)
    #    $cam.Orbit(0.05, 0) # yaw per frame
    #    [PSConsoleGL.Terminal.Drawing.Renderer3D]::RenderMesh($mesh, $cam, $fb)
    #    [Console]::Write("$ansiEsc[H")
    #    $fb.DrawToScreen()
    #    Start-Sleep -Milliseconds 1
    #}

    # Zoom in
    for ($i = 0; $i -lt 20; $i++) {
        $fb.Clear(0,0,0)
        $cam.Zoom(-0.08)
        [PSConsoleGL.Terminal.Drawing.Renderer3D]::RenderMesh($mesh, $cam, $fb)
        [Console]::Write("$ansiEsc[H")
        $fb.DrawToScreen()
        Start-Sleep -Milliseconds 1
    }

    # Pan right and up
    for ($i = 0; $i -lt 30; $i++) {
        $fb.Clear(0,0,0)
        $cam.Pan(0.03, 0.02)
        [PSConsoleGL.Terminal.Drawing.Renderer3D]::RenderMesh($mesh, $cam, $fb)
        [Console]::Write("$ansiEsc[H")
        $fb.DrawToScreen()
        Start-Sleep -Milliseconds 1
    }

    # Zoom out
    for ($i = 0; $i -lt 30; $i++) {
        $fb.Clear(0,0,0)
        $cam.Zoom(0.08)
        [PSConsoleGL.Terminal.Drawing.Renderer3D]::RenderMesh($mesh, $cam, $fb)
        [Console]::Write("$ansiEsc[H")
        $fb.DrawToScreen()
        Start-Sleep -Milliseconds 1
    }
}
finally {
    [Console]::Write("$ansiEsc[?1049l")
    Write-Host "Camera movement demo finished."
}
