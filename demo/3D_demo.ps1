try {
    # PSConsoleGL 3D demo script
    # Renders a rotating unit cube into the project's FrameBuffer and attempts to draw to the terminal
    $assembly = Join-Path $PSScriptRoot '..\src\PSConsoleGL\bin\Debug\netstandard2.0\PSConsoleGL.dll'
    if (-not (Test-Path $assembly)) {
        Write-Error "DLL not found at $assembly. Build the project first with ``dotnet build src/PSConsoleGL/PSConsoleGL.csproj``"
        exit 1
    }

    Add-Type -Path (Resolve-Path $assembly)
    $ansiEsc = [char]0x1b
    # Enter alternate screen buffer and clear once
    [Console]::Write("$ansiEsc[?1049h")
    $fb = [PSConsoleGL.Terminal.Drawing.FrameBuffer]::new()
    $aspect = [double]$fb.Width / [double]$fb.Height
    $cam = [PSConsoleGL.Terminal.Drawing.Camera]::new([PSConsoleGL.Terminal.Drawing.Vector3]::new(2,2,2), [PSConsoleGL.Terminal.Drawing.Vector3]::new(0,0,0), $aspect)
    $faceColors = [int[]]@(0xFFFF0000,0xFF00FF00,0xFF0000FF,0xFFFFFF00,0xFFFF00FF,0xFF00FFFF)
    $mesh = [PSConsoleGL.Terminal.Drawing.Mesh]::CreateUnitCube($faceColors)

    # Snapshot original vertex positions so rotation does not accumulate
    $orig = @()
    for ($i = 0; $i -lt $mesh.Vertices.Length; $i++) {
        $v = $mesh.Vertices[$i]
        $orig += ,@{X = $v.X; Y = $v.Y; Z = $v.Z}
    }

    for ($frame = 0; $frame -lt 36; $frame++) {
        $angle = $frame * (2 * [Math]::PI / 36)
        $c = [Math]::Cos($angle)
        $s = [Math]::Sin($angle)

        for ($i = 0; $i -lt $mesh.Vertices.Length; $i++) {
            $ox = $orig[$i].X; $oz = $orig[$i].Z
            $mesh.Vertices[$i].X = $ox * $c + $oz * $s
            $mesh.Vertices[$i].Z = -$ox * $s + $oz * $c
        }

        # Clear to black
        $fb.Clear(255,0,0,0)

        # Render and draw
        [PSConsoleGL.Terminal.Drawing.Renderer3D]::RenderMesh($mesh, $cam, $fb)
        [Console]::Write("$ansiEsc[H")
        $fb.DrawToScreen()

        Start-Sleep -Milliseconds 100
    }


}
finally {
    [Console]::Write("$ansiEsc[?1049l")
    Write-Host "3D demo finished."
}