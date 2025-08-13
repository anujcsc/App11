Imports System
Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.IO
Imports System.Linq
Imports System.Runtime.Serialization.Json
Imports System.Text
Imports System.Windows
Imports System.Windows.Controls
Imports OpenSilver

Partial Public Class MainPage
    Inherits UserControl

    Private persons As ObservableCollection(Of Person)
    Private Const STORAGE_KEY As String = "persons.json"

    Public Sub New()
        InitializeComponent()
        LoadPersons()
        PersonDataGrid.ItemsSource = persons
    End Sub

    Private Sub LoadPersons()
        Try
            Dim jsonObj As Object = Interop.ExecuteJavaScript($"window.localStorage.getItem('{STORAGE_KEY}')")
            Dim json As String = If(jsonObj Is Nothing, Nothing, jsonObj.ToString())

            If String.IsNullOrWhiteSpace(json) Then
                persons = New ObservableCollection(Of Person)(GetDefaultPersons())
                SavePersons()
            Else
                Dim serializer As New DataContractJsonSerializer(GetType(List(Of Person)))
                Using ms As New MemoryStream(Encoding.UTF8.GetBytes(json))
                    Dim list As List(Of Person) = TryCast(serializer.ReadObject(ms), List(Of Person))
                    persons = New ObservableCollection(Of Person)(If(list, New List(Of Person)()))
                End Using
            End If
        Catch
            ' If anything goes wrong during load, fall back to defaults
            persons = New ObservableCollection(Of Person)(GetDefaultPersons())
        End Try
    End Sub

    Private Sub SavePersons()
        Try
            Dim serializer As New DataContractJsonSerializer(GetType(List(Of Person)))
            Using ms As New MemoryStream()
                serializer.WriteObject(ms, persons.ToList())
                Dim json As String = Encoding.UTF8.GetString(ms.ToArray())
                Interop.ExecuteJavaScript($"window.localStorage.setItem('{STORAGE_KEY}', $0)", json)
            End Using
        Catch
            ' Ignore persistence errors for now
        End Try
    End Sub

    Private Function GetDefaultPersons() As IEnumerable(Of Person)
        Return New List(Of Person) From {
            New Person With {.ID = 1, .Name = "John Doe", .Age = 30},
            New Person With {.ID = 2, .Name = "Jane Smith", .Age = 25},
            New Person With {.ID = 3, .Name = "Alice Johnson", .Age = 40}
        }
    End Function

    Private Sub AddButton_Click(sender As Object, e As RoutedEventArgs)
        Dim name = (If(NameTextBox.Text, String.Empty)).Trim()
        Dim ageValue As Integer
        If String.IsNullOrWhiteSpace(name) Then
            MessageBox.Show("Please enter a name.")
            Return
        End If
        If Not Integer.TryParse((If(AgeTextBox.Text, String.Empty)).Trim(), ageValue) OrElse ageValue < 0 Then
            MessageBox.Show("Please enter a valid non-negative age.")
            Return
        End If

        Dim newId As Integer = If(persons.Any(), persons.Max(Function(p) p.ID) + 1, 1)
        persons.Add(New Person With {.ID = newId, .Name = name, .Age = ageValue})
        SavePersons()
        ClearInputs()
    End Sub

    Private Sub UpdateButton_Click(sender As Object, e As RoutedEventArgs)
        Dim selected = TryCast(PersonDataGrid.SelectedItem, Person)
        If selected Is Nothing Then
            MessageBox.Show("Please select a person to update.")
            Return
        End If

        Dim name = (If(NameTextBox.Text, String.Empty)).Trim()
        Dim ageValue As Integer
        If String.IsNullOrWhiteSpace(name) Then
            MessageBox.Show("Please enter a name.")
            Return
        End If
        If Not Integer.TryParse((If(AgeTextBox.Text, String.Empty)).Trim(), ageValue) OrElse ageValue < 0 Then
            MessageBox.Show("Please enter a valid non-negative age.")
            Return
        End If

        selected.Name = name
        selected.Age = ageValue
        SavePersons()
    End Sub

    Private Sub DeleteButton_Click(sender As Object, e As RoutedEventArgs)
        Dim selected = TryCast(PersonDataGrid.SelectedItem, Person)
        If selected Is Nothing Then
            MessageBox.Show("Please select a person to delete.")
            Return
        End If
        persons.Remove(selected)
        SavePersons()
        ClearInputs()
    End Sub

    Private Sub ClearButton_Click(sender As Object, e As RoutedEventArgs)
        ClearInputs()
    End Sub

    Private Sub PersonDataGrid_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        Dim selected = TryCast(PersonDataGrid.SelectedItem, Person)
        If selected Is Nothing Then
            ClearInputs()
            Return
        End If
        NameTextBox.Text = selected.Name
        AgeTextBox.Text = selected.Age.ToString()
    End Sub

    Private Sub ClearInputs()
        NameTextBox.Text = String.Empty
        AgeTextBox.Text = String.Empty
        PersonDataGrid.SelectedItem = Nothing
    End Sub
End Class

Public Class Person
    Implements INotifyPropertyChanged

    Private _id As Integer
    Private _name As String
    Private _age As Integer

    Public Property ID As Integer
        Get
            Return _id
        End Get
        Set(value As Integer)
            If _id <> value Then
                _id = value
                OnPropertyChanged(NameOf(ID))
            End If
        End Set
    End Property

    Public Property Name As String
        Get
            Return _name
        End Get
        Set(value As String)
            If _name <> value Then
                _name = value
                OnPropertyChanged(NameOf(Name))
            End If
        End Set
    End Property

    Public Property Age As Integer
        Get
            Return _age
        End Get
        Set(value As Integer)
            If _age <> value Then
                _age = value
                OnPropertyChanged(NameOf(Age))
            End If
        End Set
    End Property

    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

    Protected Overridable Sub OnPropertyChanged(propertyName As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
    End Sub
End Class