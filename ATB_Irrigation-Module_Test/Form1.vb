Imports System.Reflection

Public Class Form1
    Public Sub New()
        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Dim plantNames = atbApi.data.LocalPlantDb.Instance.getPlantNames()

        For i = 0 To plantNames.Count - 1
            ComboBox1.Items.Add(plantNames(i))
        Next

        ComboBox1.Sorted = True

        ' Add any initialization after the InitializeComponent() call.
        Dim soilNames = atbApi.data.LocalSoilDb.Instance.getSoilNames()

        For i = 0 To soilNames.Count - 1
            ComboBox2.Items.Add(soilNames(i))
        Next

        ComboBox2.Sorted = True
    End Sub


    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim plant As atbApi.data.Plant = New atbApi.data.Plant(ComboBox1.SelectedItem)

        For i = 400 To 399
            Dim plantSet As atbApi.data.PlantValues = plant.getValues(i)
            If IsNothing(plantSet) Then
                Continue For
            End If
            For Each _property As PropertyInfo In plantSet.GetType().GetProperties()
                If _property.GetValue(plantSet, Nothing) <> Nothing Then
                    TextBox1.AppendText(i.ToString() + ": " + _property.Name + ": " + _property.GetValue(plantSet, Nothing).ToString() + vbNewLine)
                End If
            Next
        Next

        Dim seedDate As Date = DateSerial(2004, 4, 13)
        Dim harvestDate As Date = DateSerial(2004, 10, 5)
        For day = 9 To 15
            Dim actualDate As Date = DateSerial(2004, 4, day)
            Dim plantSet As atbApi.data.PlantValues = plant.getValues(actualDate, seedDate, harvestDate)
            If IsNothing(plantSet) Then
                Continue For
            End If
            For Each _property As PropertyInfo In plantSet.GetType().GetProperties()
                If _property.GetValue(plantSet, Nothing) <> Nothing Then
                    TextBox1.AppendText(actualDate.ToString() + ": " + _property.Name + ": " + _property.GetValue(plantSet, Nothing).ToString() + vbNewLine)
                End If
            Next
        Next
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Dim fs As System.IO.FileStream = New IO.FileStream("C:\IWRM_MIKE-Basin_Irrigation-Module\ATB_Irrigation-Module_cs\IWRM_ATB-PlantData.csv", IO.FileMode.Open)
        Dim plantDb As atbApi.data.PlantDb = New atbApi.data.PlantDb(fs)
        fs.Close()
        Dim filePlant As atbApi.data.Plant = New atbApi.data.Plant("CROPWAT_80_Crop_data_CITRUS_70p_ca_bare", plantDb)
        TextBox1.AppendText(filePlant.stageTotal.ToString() + " " + filePlant.name + vbNewLine)
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Dim soil As atbApi.data.Soil = New atbApi.data.Soil(ComboBox2.SelectedItem)

        For z = 0 To 2 Step 0.01
            Dim soilSet As atbApi.data.SoilValues = soil.getValues(z)
            If IsNothing(soilSet) Then
                Continue For
            End If
            For Each _property As PropertyInfo In soilSet.GetType().GetProperties()
                If _property.GetValue(soilSet, Nothing) <> Nothing Then
                    TextBox1.AppendText(z.ToString() + ": " + _property.Name + ": " + _property.GetValue(soilSet, Nothing).ToString() + vbNewLine)
                End If
            Next
        Next
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        Dim climate As atbApi.data.Climate = New atbApi.data.Climate(atbApi.data.TimeStep.day)
        Dim fs As System.IO.FileStream = New IO.FileStream("C:\IWRM_MIKE-Basin_Irrigation-Module\testdata\climate.public_service.date_2012-04-12T00_00_00.000Z.csv", IO.FileMode.Open)
        climate.loadFromFileStream(fs)
        fs.Close()
        TextBox1.AppendText(climate.name + " start:" + climate.start + " end:" + climate.end + vbNewLine)
    End Sub

    Private Async Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        Dim climate As atbApi.data.Climate = New atbApi.data.Climate(atbApi.data.TimeStep.day)
        Dim count As Integer = Await climate.loadFromATBWebService(New atbApi.Location(12.34, -80.3), New DateTime(2012, 1, 1, 0, 0, 0, DateTimeKind.Utc), New DateTime(2012, 12, 31, 0, 0, 0, DateTimeKind.Utc))
        TextBox1.AppendText(climate.name + " start:" + climate.start + " end:" + climate.end + vbNewLine)

        Dim climate2 As atbApi.data.Climate = New atbApi.data.Climate(atbApi.data.TimeStep.month)
        Dim count2 As Integer = Await climate2.loadFromATBWebService(New atbApi.Location(12.34, -80.3), "uea_cru_public", New DateTime(2012, 1, 1, 0, 0, 0, DateTimeKind.Utc), New DateTime(2012, 12, 31, 0, 0, 0, DateTimeKind.Utc))
        TextBox1.AppendText(climate2.name + " start:" + climate2.start + " end:" + climate2.end + vbNewLine)
    End Sub

    Private Async Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click
        Dim location As atbApi.Location = New atbApi.Location(48.5, 9.3)
        Dim climate As atbApi.data.Climate = New atbApi.data.Climate(atbApi.data.TimeStep.day)
        Dim count As Integer = Await climate.loadFromATBWebService(location, New DateTime(2012, 1, 1, 0, 0, 0, DateTimeKind.Utc), New DateTime(2012, 12, 31, 0, 0, 0, DateTimeKind.Utc))
        Dim altitude As Double = Await climate.loadAltitudeFromATBWebService(location)
        location.alt = altitude
        TextBox1.AppendText(climate.name + " start:" + climate.start + " end:" + climate.end + " altitude:" + location.alt.ToString() + vbNewLine)
        Dim testDate As DateTime = New DateTime(2012, 7, 31, 0, 0, 0, DateTimeKind.Utc)
        Dim et0Hg As atbApi.Et0Result = atbApi.Et0.Et0CalcHg(climate, testDate, location, New atbApi.Et0HgArgs)
        Dim et0Pm As atbApi.Et0Result = atbApi.Et0.Et0CalcPm(climate, testDate, location, New atbApi.Et0PmArgs)
        Dim et0 As atbApi.Et0Result = atbApi.Et0.Et0Calc(climate, testDate, location)

        TextBox1.AppendText("et0Hg:" + et0Hg.et0.ToString() + " et0Pm:" + et0Pm.et0.ToString() + " et0:" + et0.et0.ToString() + " climateEt0:" + climate.getValues(testDate).et0.ToString() + vbNewLine)
    End Sub




    Private Async Sub Button7_Click(sender As Object, e As EventArgs) Handles Button7.Click
        'create location to get nearest climate station from webservice
        Dim location As atbApi.Location = New atbApi.Location(48.5, 9.3)
        'create climate with daily tmestep
        Dim climate As atbApi.data.Climate = New atbApi.data.Climate(atbApi.data.TimeStep.day)
        'define interval to load climate data
        Dim climateStart As DateTime = New DateTime(2012, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        Dim climateEnd As DateTime = New DateTime(2012, 12, 31, 0, 0, 0, DateTimeKind.Utc)
        'load data asyncron from webservice
        Dim count As Integer = Await climate.loadFromATBWebService(location, climateStart, climateEnd)
        Dim loopDate As DateTime = climateStart
        While (loopDate <= climateEnd)
            Dim values As atbApi.data.ClimateValues = New atbApi.data.ClimateValues()
            values.max_temp = climate.getValues(loopDate).max_temp
            values.min_temp = climate.getValues(loopDate).min_temp
            climate.addValues(loopDate, values)
            loopDate = loopDate.AddDays(1)
        End While

        'load altitude from webservice
        Dim altitude As Double = Await climate.loadAltitudeFromATBWebService(location)
        location.alt = altitude
        'create plant from dll internal plant database
        Dim plant As atbApi.data.Plant = New atbApi.data.Plant(ComboBox1.SelectedItem)
        'create soil from dll internal standard soils
        Dim soil As atbApi.data.Soil = New atbApi.data.Soil(ComboBox2.SelectedItem)
        'create new soil water conditions if sequential calculation starts
        'it is important, to keep transpirationResult.lastConditions and use this
        'instead for consecutive crops on the same field
        Dim soilConditions As atbApi.data.SoilConditions = New atbApi.data.SoilConditions()
        'define seedDate and harvestDate
        Dim seedDate As DateTime = New DateTime(2012, 4, 12, 0, 0, 0, DateTimeKind.Utc)
        Dim harvestDate As DateTime = New DateTime(2012, 10, 5, 0, 0, 0, DateTimeKind.Utc)
        'start calculation
        '        Dim transpirationResult As atbApi.TranspirationResult = atbApi.Transpiration.TranspirationCalc(climate, plant, soil, Nothing, location, seedDate, harvestDate, soilConditions, False)
        '       TextBox1.AppendText("et0:" + transpirationResult.et0.ToString() + " runtimeMs:" + transpirationResult.runtimeMs.ToString("F3") + vbNewLine)
        'keep this for next calculation on this field
        '      Dim nextSoilConditions As atbApi.data.SoilConditions = transpirationResult.lastConditions
    End Sub
End Class
