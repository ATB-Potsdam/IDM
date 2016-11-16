Imports System.Reflection
Imports System.Threading.Tasks
Imports System.Globalization
Imports System.IO



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
        Dim fs As System.IO.FileStream = New IO.FileStream("C:\IWRM_MIKE-Basin_Irrigation-Module\testdata\climate_Potsdam_53c6b30845900e364c000013.date_1980-01-01T00_00_00.000Z.csv", IO.FileMode.Open)
        Dim climate As atbApi.data.Climate = New atbApi.data.Climate(fs, atbApi.data.TimeStep.day)
        fs.Close()
        TextBox1.AppendText(climate.name + " start:" + climate.start + " end:" + climate.end + vbNewLine)

        For i = 1 To 6
            Dim fs1 As System.IO.FileStream = New IO.FileStream("C:\IWRM_MIKE-Basin_Irrigation-Module\testdata\climate.uea_cru_public.date-1900-01-01T00-00-00.000Z_clean_" & i & ".csv", IO.FileMode.Open)
            Dim climate1 As atbApi.data.Climate = New atbApi.data.Climate(fs1, atbApi.data.TimeStep.month)
            fs1.Close()
            TextBox1.AppendText(climate1.name + " start:" + climate1.start + " end:" + climate1.end + vbNewLine)

        Next
    End Sub

    Private Async Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        Dim climate As atbApi.data.Climate = New atbApi.data.Climate("12.34_-80.3", atbApi.data.TimeStep.day)
        Dim location As atbApi.Location = New atbApi.Location(12.34, -80.3)
        Dim count As Integer = Await climate.loadClimateByLocationTagFromATBWebService(location, Nothing, New DateTime(2012, 1, 1, 0, 0, 0, DateTimeKind.Utc), New DateTime(2012, 12, 31, 0, 0, 0, DateTimeKind.Utc))
        TextBox1.AppendText(climate.name + " start:" + climate.start + " end:" + climate.end + vbNewLine)

        Dim climate2 As atbApi.data.Climate = New atbApi.data.Climate("12.34_-80.3_uea", atbApi.data.TimeStep.month)
        Dim count2 As Integer = Await climate2.loadClimateByLocationTagFromATBWebService(location, "uea_cru_public", New DateTime(2012, 1, 1, 0, 0, 0, DateTimeKind.Utc), New DateTime(2012, 12, 31, 0, 0, 0, DateTimeKind.Utc))
        TextBox1.AppendText(climate2.name + " start:" + climate2.start + " end:" + climate2.end + vbNewLine)
    End Sub

    Private Async Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click
        Dim location As atbApi.Location = New atbApi.Location(48.5, 9.3)
        Dim climate As atbApi.data.Climate = New atbApi.data.Climate("48.5_9.3", atbApi.data.TimeStep.day)
        Dim count As Integer = Await climate.loadClimateByLocationTagFromATBWebService(location, Nothing, New DateTime(2012, 1, 1, 0, 0, 0, DateTimeKind.Utc), New DateTime(2012, 12, 31, 0, 0, 0, DateTimeKind.Utc))
        Dim altitude As Double = Await climate.loadAltitudeFromATBWebService(location)
        location.alt = altitude
        TextBox1.AppendText(climate.name + " start:" + climate.start + " end:" + climate.end + " altitude:" + location.alt.ToString() + vbNewLine)
        Dim testDate As DateTime = New DateTime(2012, 7, 31, 0, 0, 0, DateTimeKind.Utc)
        Dim et0Hg As atbApi.Et0Result = New atbApi.Et0Result
        atbApi.Et0.Et0CalcHg(climate, testDate, location, New atbApi.Et0HgArgs, et0Hg)
        Dim et0Pm As atbApi.Et0Result = New atbApi.Et0Result
        atbApi.Et0.Et0CalcPm(climate, testDate, location, New atbApi.Et0PmArgs, et0Pm)
        Dim et0 As atbApi.Et0Result = New atbApi.Et0Result
        atbApi.Et0.Et0Calc(climate, testDate, location, et0)

        TextBox1.AppendText("et0Hg:" + et0Hg.et0.ToString() + " et0Pm:" + et0Pm.et0.ToString() + " et0:" + et0.et0.ToString() + " climateEt0:" + climate.getValues(testDate).et0.ToString() + vbNewLine)
    End Sub




    Private Async Sub Button7_Click(sender As Object, e As EventArgs) Handles Button7.Click
        Dim climate As atbApi.data.Climate = New atbApi.data.Climate("DWD_03987_Potsdam", "53c6b30845900e364c000013", atbApi.data.TimeStep.day)
        'define interval to load climate data
        Dim climateStart As DateTime = New DateTime(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        Dim climateEnd As DateTime = New DateTime(2015, 12, 31, 0, 0, 0, DateTimeKind.Utc)
        'load data asyncron from webservice
        Dim count As Integer = Await climate.loadClimateByIdFromATBWebService(climateStart, climateEnd)

        'load altitude from webservice
        'Dim altitude As Double = Await climate.loadAltitudeFromATBWebService(climate.location)
        'create plant from dll internal plant database
        Dim plant As atbApi.data.Plant = New atbApi.data.Plant("ATB_barley_oats_wheat_april")
        'create soil from dll internal standard soils
        Dim soil As atbApi.data.Soil = New atbApi.data.Soil("USDA-soilclass_sandy_loam")
        'create new soil water conditions if sequential calculation starts
        'it is important, to keep transpirationResult.lastConditions and use this
        'instead for consecutive crops on the same field
        Dim soilConditions As atbApi.data.SoilConditions = New atbApi.data.SoilConditions()
        'define seedDate and harvestDate
        Dim seedDate As DateTime = New DateTime(2015, 4, 12, 0, 0, 0, DateTimeKind.Utc)
        Dim harvestDate As DateTime = New DateTime(2015, 10, 5, 0, 0, 0, DateTimeKind.Utc)
        'start calculation
        Dim etResult As atbApi.ETResult = New atbApi.ETResult
        Dim etArgs As atbApi.ETArgs = New atbApi.ETArgs

        '        Dim transpirationResult As atbApi.TranspirationResult = atbApi.Transpiration.TranspirationCalc(climate, plant, soil, Nothing, location, seedDate, harvestDate, soilConditions, False)
        '       TextBox1.AppendText("et0:" + transpirationResult.et0.ToString() + " runtimeMs:" + transpirationResult.runtimeMs.ToString("F3") + vbNewLine)
        'keep this for next calculation on this field
        '      Dim nextSoilConditions As atbApi.data.SoilConditions = transpirationResult.lastConditions
    End Sub


    Private Async Sub Button8_Click(sender As Object, e As EventArgs) Handles Button8.Click
        Dim climateNames = Await atbApi.data.ClimateWebServiceDb.getClimateNames(False, Nothing)
        For i = 0 To climateNames.Count - 1
            ComboBox3.Items.Add(climateNames(i))
        Next

        ComboBox3.Sorted = True

        'Dim seedDate As DateTime = New DateTime(2012, 4, 12, 0, 0, 0, DateTimeKind.Utc)
        'Dim harvestDate As DateTime = New DateTime(2012, 10, 5, 0, 0, 0, DateTimeKind.Utc)
        'Dim testClimate As Task(Of atbApi.data.Climate) = atbApi.data.ClimateDb.getClimate("DWD_04371_Salzuflen_Bad", seedDate, harvestDate, atbApi.data.TimeStep.day)
    End Sub

    Private Sub ComboBox3_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox3.SelectedIndexChanged

    End Sub

    Private Sub Button9_Click(sender As Object, e As EventArgs) Handles Button9.Click
        'transpiration calc DHI
        Dim climateDb As atbApi.data.ClimateDb = New atbApi.data.ClimateDb()
        For i = 1 To 6
            Dim climateFile = "..\..\..\testdata\climate.uea_cru_public.date-1900-01-01T00-00-00.000Z_clean_" & i & ".csv"
            Dim climateStream As FileStream = File.OpenRead(climateFile)
            climateDb.addClimate(climateStream, atbApi.data.TimeStep.month)
        Next

        Dim cSFile = "..\..\..\testdata\DHI_Field_IWRM_cropSequences.csv"
        Dim cSStream As FileStream = File.OpenRead(cSFile)
        Dim cS As atbApi.data.CropSequence = _
            New atbApi.data.CropSequence(cSStream, atbApi.data.LocalPlantDb.Instance, atbApi.data.LocalSoilDb.Instance, climateDb)


        Dim startDate As DateTime = New DateTime(2015, 4, 12, 0, 0, 0, DateTimeKind.Utc)
        Dim endDate As DateTime = New DateTime(2015, 10, 5, 0, 0, 0, DateTimeKind.Utc)
        Dim etArgs As New atbApi.ETArgs()
        Dim result As IDictionary(Of String, atbApi.ETResult)
        result = cS.runCropSequence(start:=startDate, end:=endDate, step:=atbApi.data.TimeStep.month, etArgs:=etArgs)
    End Sub
End Class
